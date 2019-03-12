using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

namespace MobilePipeline.Pipeline
{
    public class MobilePipeline : RenderPipeline
    {
        private readonly MobilePipelineAsset _pipelineAsset;
        private const int MaxVisibleLights = 16;

        private static readonly int LightColorsId = Shader.PropertyToID("_VisibleLightColors");
        private static readonly int LightDirectionsOrPositionsId = Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
        private static readonly int LightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
        private static readonly int LightSpotDirectionsId = Shader.PropertyToID("_VisibleLightSpotDirections");
        private static readonly int LightIndicesOffsetAndCountId = Shader.PropertyToID("unity_LightIndicesOffsetAndCount");
        private static readonly int WorldSpaceCameraPosId = Shader.PropertyToID("_WorldSpaceCameraPos");

        private readonly Vector4[] _lightColors = new Vector4[MaxVisibleLights];
        private readonly Vector4[] _lightDirectionsOrPositions = new Vector4[MaxVisibleLights];
        private readonly Vector4[] _lightAttenuations = new Vector4[MaxVisibleLights];
        private readonly Vector4[] _lightSpotDirections = new Vector4[MaxVisibleLights];

        private CullResults _cullResults;
        private Material _errorMaterial;
        private int _shadowTileCount;

        private readonly DrawRendererFlags _drawFlags;
        private readonly CommandBuffer _cameraBuffer;

        public MobilePipeline(MobilePipelineAsset pipelineAsset)
        {
            _pipelineAsset = pipelineAsset;
            _cameraBuffer = new CommandBuffer {name = "Render Camera"};
            if (_pipelineAsset.DynamicBatching)
            {
                _drawFlags = DrawRendererFlags.EnableDynamicBatching;
            }

            if (_pipelineAsset.Instancing)
            {
                _drawFlags |= DrawRendererFlags.EnableInstancing;
            }
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
            _cameraBuffer.SetGlobalVector(WorldSpaceCameraPosId, camera.transform.position);
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

            var filterSettings = new FilterRenderersSettings(true);

            if(_pipelineAsset.DrawOpaque)
            {
                drawSettings.sorting.flags = SortFlags.CommonOpaque;
                filterSettings.renderQueueRange = RenderQueueRange.opaque;
                context.DrawRenderers(_cullResults.visibleRenderers, ref drawSettings, filterSettings);
            }
            
            if(_pipelineAsset.DrawSkybox)
            {
                context.DrawSkybox(camera);
            }

            if(_pipelineAsset.DrawTransparent)
            {
                drawSettings.sorting.flags = SortFlags.CommonTransparent;
                filterSettings.renderQueueRange = RenderQueueRange.transparent;
                context.DrawRenderers(_cullResults.visibleRenderers, ref drawSettings, filterSettings);
            }

            DrawDefaultPipeline(context, camera);

            _cameraBuffer.EndSample("Render Camera");
            context.ExecuteCommandBuffer(_cameraBuffer);
            _cameraBuffer.Clear();

            context.Submit();
        }

        private void ConfigureLights()
        {
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
                    }
                }

                _lightAttenuations[i] = attenuation;
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