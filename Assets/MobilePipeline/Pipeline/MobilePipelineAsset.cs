using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

namespace MobilePipeline.Pipeline
{
    [CreateAssetMenu(menuName = "Rendering/Mobile Pipeline")]
    public class MobilePipelineAsset : RenderPipelineAsset
    {
        private enum ShadowMapSize
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096
        }

        [FormerlySerializedAs("shadowMapSize")]
        [SerializeField]
        private ShadowMapSize _shadowMapSize = ShadowMapSize._1024;

        [SerializeField]
        private bool _dynamicBatching;

        [SerializeField]
        private bool _instancing;

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new MobilePipeline(_dynamicBatching, _instancing, (int) _shadowMapSize);
        }
    }
}