using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

namespace MobilePipeline.Pipeline
{
    [CreateAssetMenu(menuName = "Rendering/Mobile Pipeline")]
    public class MobilePipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private bool _dynamicBatching = true;

        [SerializeField]
        private bool _instancing = true;

        [SerializeField]
        private bool _drawOpaque = true;

        [SerializeField]
        private bool _drawTransparent = true;

        [SerializeField]
        private bool _drawSkybox = true;

        public bool DynamicBatching => _dynamicBatching;

        public bool Instancing => _instancing;

        public bool DrawSkybox => _drawSkybox;

        public bool DrawTransparent => _drawTransparent;

        public bool DrawOpaque => _drawOpaque;

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new MobilePipeline(this);
        }
    }
}