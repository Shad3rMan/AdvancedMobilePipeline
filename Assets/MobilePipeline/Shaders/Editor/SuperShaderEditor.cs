using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MobilePipeline.Shaders.Editor
{
    public class SuperShaderEditor : ShaderGUI
    {
        private enum DrawMode
        {
            Opaque,
            Transparent
        }

        private Object[] _materials;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            _materials = materialEditor.targets;
            if (_materials != null)
            {
                base.OnGUI(materialEditor, props);
                DrawPresetSelector();
            }
        }

        private void DrawPresetSelector()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Preset Selection");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Opaque"))
            {
                ApplyDrawMode(DrawMode.Opaque);
            }
            
            if (GUILayout.Button("Transparent"))
            {
                ApplyDrawMode(DrawMode.Transparent);
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void ApplyDrawMode(DrawMode mode)
        {
            foreach (Material material in _materials)
            {
                switch (mode)
                {
                    case DrawMode.Opaque:
                        material.SetOverrideTag("RenderType", "Geometry");
                        material.SetInt(Properties.SrcBlend, (int) BlendMode.One);
                        material.SetInt(Properties.DstBlend, (int) BlendMode.Zero);
                        material.SetInt(Properties.ZWrite, 1);
                        material.renderQueue = (int) RenderQueue.Geometry;
                        break;
                    case DrawMode.Transparent:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt(Properties.SrcBlend, (int) BlendMode.SrcAlpha);
                        material.SetInt(Properties.DstBlend, (int) BlendMode.OneMinusSrcAlpha);
                        material.SetInt(Properties.ZWrite, 0);
                        material.renderQueue = (int) RenderQueue.Transparent;
                        break;
                }
            }
        }

        private static class Properties
        {
            public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
            public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
            public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        }
    }
}