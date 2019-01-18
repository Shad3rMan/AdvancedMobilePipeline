using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace MobilePipeline.Pipeline
{
    public class MobilePipeline : RenderPipeline
    {
        private const int MaxVisibleLights = 16;

        private static readonly int LightColorsId = Shader.PropertyToID("_VisibleLightColors");
        private static readonly int LightDirectionsOrPositionsId = Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
        private static readonly int LightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
        private static readonly int LightSpotDirectionsId = Shader.PropertyToID("_VisibleLightSpotDirections");
        private static readonly int LightIndicesOffsetAndCountId = Shader.PropertyToID("unity_LightIndicesOffsetAndCount");
        private static readonly int ShadowMapId = Shader.PropertyToID("_ShadowMap");
        private static readonly int WorldToShadowMatricesId = Shader.PropertyToID("_WorldToShadowMatrices");
        private static readonly int ShadowBiasId = Shader.PropertyToID("_ShadowBias");
        private static readonly int ShadowDataId = Shader.PropertyToID("_ShadowData");
        private static readonly int ShadowMapSizeId = Shader.PropertyToID("_ShadowMapSize");

        private readonly Vector4[] _lightColors = new Vector4[MaxVisibleLights];
        private readonly Vector4[] _lightDirectionsOrPositions = new Vector4[MaxVisibleLights];
        private readonly Vector4[] _lightAttenuations = new Vector4[MaxVisibleLights];
        private readonly Vector4[] _lightSpotDirections = new Vector4[MaxVisibleLights];
        private readonly Vector4[] _shadowData = new Vector4[MaxVisibleLights];
        private readonly Matrix4x4[] _worldToShadowMatrices = new Matrix4x4[MaxVisibleLights];

        private CullResults _cullResults;
        private RenderTexture _shadowMap;
        private Material _errorMaterial;
        private int _shadowTileCount;

        private readonly DrawRendererFlags _drawFlags;
        private readonly CommandBuffer _cameraBuffer;
        private readonly CommandBuffer _shadowBuffer;
        private readonly int _shadowMapSize;

        public MobilePipeline(bool dynamicBatching, bool instancing, int shadowMapSize)
        {
            _shadowBuffer = new CommandBuffer {name = "Render Shadows"};
            _cameraBuffer = new CommandBuffer {name = "Render Camera"};
            if (dynamicBatching)
            {
                _drawFlags = DrawRendererFlags.EnableDynamicBatching;
            }

            if (instancing)
            {
                _drawFlags |= DrawRendererFlags.EnableInstancing;
            }

            _shadowMapSize = shadowMapSize;
        }

        public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            base.Render(renderContext, cameras);

            foreach (var camera in cameras)
            {
                Render(renderContext, camera);
            }
        }

        private void Render(ScriptableRenderContext context, Camera camera)
        {
            if (!CullResults.GetCullingParameters(camera, out var cullingParameters))
            {
                return;
            }

            cullingParameters.shadowDistance = 20f;

#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
#endif

            CullResults.Cull(ref cullingParameters, context, ref _cullResults);
            if (_cullResults.visibleLights.Count > 0)
            {
                ConfigureLights();
                RenderShadows(context);
            }
            else
            {
                _cameraBuffer.SetGlobalVector(LightIndicesOffsetAndCountId, Vector4.zero);
            }

            context.SetupCameraProperties(camera);

            var clearFlags = camera.clearFlags;
            _cameraBuffer.ClearRenderTarget(
                (clearFlags & CameraClearFlags.Depth) != 0,
                (clearFlags & CameraClearFlags.Color) != 0,
                camera.backgroundColor
            );

            _cameraBuffer.BeginSample("Render Camera");
            _cameraBuffer.SetGlobalVectorArray(LightColorsId, _lightColors);
            _cameraBuffer.SetGlobalVectorArray(LightDirectionsOrPositionsId, _lightDirectionsOrPositions);
            _cameraBuffer.SetGlobalVectorArray(LightAttenuationsId, _lightAttenuations);
            _cameraBuffer.SetGlobalVectorArray(LightSpotDirectionsId, _lightSpotDirections);
            context.ExecuteCommandBuffer(_cameraBuffer);
            _cameraBuffer.Clear();

            var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"))
            {
                flags = _drawFlags
            };

            if (_cullResults.visibleLights.Count > 0)
            {
                drawSettings.rendererConfiguration = RendererConfiguration.PerObjectLightIndices8;
            }

            drawSettings.sorting.flags = SortFlags.CommonOpaque;

            var filterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.opaque
            };

            context.DrawRenderers(_cullResults.visibleRenderers, ref drawSettings, filterSettings);

            context.DrawSkybox(camera);

            drawSettings.sorting.flags = SortFlags.CommonTransparent;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(_cullResults.visibleRenderers, ref drawSettings, filterSettings);

            DrawDefaultPipeline(context, camera);

            _cameraBuffer.EndSample("Render Camera");
            context.ExecuteCommandBuffer(_cameraBuffer);
            _cameraBuffer.Clear();

            context.Submit();

            if (_shadowMap != null)
            {
                RenderTexture.ReleaseTemporary(_shadowMap);
                _shadowMap = null;
            }
        }

        private void ConfigureLights()
        {
            _shadowTileCount = 0;
            for (int i = 0; i < _cullResults.visibleLights.Count; i++)
            {
                if (i == MaxVisibleLights)
                {
                    break;
                }

                var light = _cullResults.visibleLights[i];
                _lightColors[i] = light.finalColor;
                var attenuation = Vector4.zero;
                attenuation.w = 1f;
                var shadow = Vector4.zero;

                if (light.lightType == LightType.Directional)
                {
                    var v = light.localToWorld.GetColumn(2);
                    v.x = -v.x;
                    v.y = -v.y;
                    v.z = -v.z;
                    _lightDirectionsOrPositions[i] = v;
                }
                else
                {
                    _lightDirectionsOrPositions[i] = light.localToWorld.GetColumn(3);
                    attenuation.x = 1f / Mathf.Max(light.range * light.range, 0.00001f);

                    var shadowLight = light.light;
                    if (light.lightType == LightType.Spot)
                    {
                        var v = light.localToWorld.GetColumn(2);
                        v.x = -v.x;
                        v.y = -v.y;
                        v.z = -v.z;
                        _lightSpotDirections[i] = v;

                        float outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
                        float outerCos = Mathf.Cos(outerRad);
                        float outerTan = Mathf.Tan(outerRad);
                        float innerCos =
                            Mathf.Cos(Mathf.Atan((46f / 64f) * outerTan));
                        float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
                        attenuation.z = 1f / angleRange;
                        attenuation.w = -outerCos * attenuation.z;

                        if (shadowLight.shadows != LightShadows.None && _cullResults.GetShadowCasterBounds(i, out _))
                        {
                            _shadowTileCount += 1;
                            shadow.x = shadowLight.shadowStrength;
                        }
                    }
                }

                _lightAttenuations[i] = attenuation;
                _shadowData[i] = shadow;
            }

            if (_cullResults.visibleLights.Count > MaxVisibleLights)
            {
                var lightIndices = _cullResults.GetLightIndexMap();
                for (int i = MaxVisibleLights; i < _cullResults.visibleLights.Count; i++)
                {
                    lightIndices[i] = -1;
                }

                _cullResults.SetLightIndexMap(lightIndices);
            }
        }

        private void RenderShadows(ScriptableRenderContext context)
        {
            int split;
            if (_shadowTileCount <= 1)
            {
                split = 1;
            }
            else if (_shadowTileCount <= 4)
            {
                split = 2;
            }
            else if (_shadowTileCount <= 9)
            {
                split = 3;
            }
            else
            {
                split = 4;
            }

            var tileSize = (float) _shadowMapSize / split;
            var tileScale = 1f / split;
            var tileViewport = new Rect(0f, 0f, tileSize, tileSize);

            _shadowMap = RenderTexture.GetTemporary(_shadowMapSize, _shadowMapSize, 16, RenderTextureFormat.Shadowmap);
            _shadowMap.filterMode = FilterMode.Bilinear;
            _shadowMap.wrapMode = TextureWrapMode.Clamp;

            CoreUtils.SetRenderTarget(_shadowBuffer, _shadowMap, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Depth);
            _shadowBuffer.BeginSample("Render Shadows");

            int tileIndex = 0;
            for (int i = 0; i < _cullResults.visibleLights.Count; i++)
            {
                if (i == MaxVisibleLights)
                {
                    break;
                }

                if (_shadowData[i].x <= 0f)
                {
                    continue;
                }

                if (!_cullResults.ComputeSpotShadowMatricesAndCullingPrimitives(i, out var viewMatrix, out var projectionMatrix, out _))
                {
                    _shadowData[i].x = 0f;
                    continue;
                }

                var tileOffsetX = tileIndex % split;
                var tileOffsetY = (float) tileIndex / split;
                tileViewport.x = tileOffsetX * tileSize;
                tileViewport.y = tileOffsetY * tileSize;
                if (split > 1)
                {
                    _shadowBuffer.SetViewport(tileViewport);
                    _shadowBuffer.EnableScissorRect(new Rect(tileViewport.x + 4f, tileViewport.y + 4f, tileSize - 8f, tileSize - 8f));
                }

                _shadowBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                _shadowBuffer.SetGlobalFloat(ShadowBiasId, _cullResults.visibleLights[i].light.shadowBias);
                context.ExecuteCommandBuffer(_shadowBuffer);
                _shadowBuffer.Clear();

                var shadowSettings = new DrawShadowsSettings(_cullResults, i);
                context.DrawShadows(ref shadowSettings);

                if (SystemInfo.usesReversedZBuffer)
                {
                    projectionMatrix.m20 = -projectionMatrix.m20;
                    projectionMatrix.m21 = -projectionMatrix.m21;
                    projectionMatrix.m22 = -projectionMatrix.m22;
                    projectionMatrix.m23 = -projectionMatrix.m23;
                }

                var scaleOffset = Matrix4x4.identity;
                scaleOffset.m00 = scaleOffset.m11 = scaleOffset.m22 = 0.5f;
                scaleOffset.m03 = scaleOffset.m13 = scaleOffset.m23 = 0.5f;
                _worldToShadowMatrices[i] =
                    scaleOffset * (projectionMatrix * viewMatrix);

                if (split > 1)
                {
                    var tileMatrix = Matrix4x4.identity;
                    tileMatrix.m00 = tileMatrix.m11 = tileScale;
                    tileMatrix.m03 = tileOffsetX * tileScale;
                    tileMatrix.m13 = tileOffsetY * tileScale;
                    _worldToShadowMatrices[i] = tileMatrix * _worldToShadowMatrices[i];
                }

                tileIndex += 1;
            }

            if (split > 1)
            {
                _shadowBuffer.DisableScissorRect();
            }

            _shadowBuffer.SetGlobalTexture(ShadowMapId, _shadowMap);
            _shadowBuffer.SetGlobalMatrixArray(WorldToShadowMatricesId, _worldToShadowMatrices);
            _shadowBuffer.SetGlobalVectorArray(ShadowDataId, _shadowData);
            float invShadowMapSize = 1f / _shadowMapSize;
            _shadowBuffer.SetGlobalVector(ShadowMapSizeId, new Vector4(invShadowMapSize, invShadowMapSize, _shadowMapSize, _shadowMapSize));
            _shadowBuffer.EndSample("Render Shadows");
            context.ExecuteCommandBuffer(_shadowBuffer);
            _shadowBuffer.Clear();
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera)
        {
            if (_errorMaterial == null)
            {
                Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
                _errorMaterial = new Material(errorShader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            var drawSettings = new DrawRendererSettings(
                camera, new ShaderPassName("ForwardBase")
            );
            drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
            drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
            drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
            drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
            drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
            drawSettings.SetOverrideMaterial(_errorMaterial, 0);

            var filterSettings = new FilterRenderersSettings(true);

            context.DrawRenderers(
                _cullResults.visibleRenderers, ref drawSettings, filterSettings
            );
        }
    }
}