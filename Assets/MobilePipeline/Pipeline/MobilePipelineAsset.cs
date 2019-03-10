using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

namespace MobilePipeline.Pipeline
{
    [CreateAssetMenu(menuName = "Rendering/Mobile Pipeline")]
    public class MobilePipelineAsset : RenderPipelineAsset
    {
        public enum ShadowMapSizeTypes
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096
        }

        [SerializeField] private ShadowMapSizeTypes _shadowMapSize = ShadowMapSizeTypes._1024;

        [SerializeField] private bool _dynamicBatching;

        [SerializeField] private bool _instancing;

        [SerializeField] private bool _drawOpaque = true;

        [SerializeField] private bool _drawTransparent = true;

        [SerializeField] private bool _drawSkybox = true;

        public ShadowMapSizeTypes ShadowMapSize
        {
            get { return _shadowMapSize; }
        }

        public bool DynamicBatching
        {
            get { return _dynamicBatching; }
        }

        public bool Instancing
        {
            get { return _instancing; }
        }

        public bool DrawSkybox
        {
            get { return _drawSkybox; }
        }

        public bool DrawTransparent
        {
            get { return _drawTransparent; }
        }

        public bool DrawOpaque
        {
            get { return _drawOpaque; }
        }

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new MobilePipeline(this);
        }
    }
}