using UnityEditor;
using UnityEngine;

namespace MobilePipeline.Utils
{
    [ExecuteAlways]
    public class SuperLight : MonoBehaviour
    {
        [SerializeField]
        private Color _color;

        [SerializeField]
        private float _intensity;
        
        private static readonly int LightDirection = Shader.PropertyToID("_LightDirection");
        private static readonly int LightColor = Shader.PropertyToID("_LightColor");

        private void Update()
        {
            var v = transform.localToWorldMatrix.GetColumn(2);
            v.x = -v.x;
            v.y = -v.y;
            v.z = -v.z;
            Shader.SetGlobalVector(LightDirection, v);
            Shader.SetGlobalColor(LightColor, _color * _intensity);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _color;
            Gizmos.DrawRay(transform.position, transform.forward*2);
            Gizmos.DrawIcon(transform.position, "DirectionalLight Gizmo", true);
        }
    }
}