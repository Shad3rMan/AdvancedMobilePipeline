using UnityEngine;

namespace MobilePipeline.Utils
{
    public class InstancedColor : MonoBehaviour
    {
        [SerializeField]
        private Color _color = Color.white;

        private static MaterialPropertyBlock _propertyBlock;
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            var propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", _color);
            GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
        }

        private void OnValidate()
        {
            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            _propertyBlock.SetColor(ColorId, _color);
            GetComponent<MeshRenderer>().SetPropertyBlock(_propertyBlock);
        }
    }
}