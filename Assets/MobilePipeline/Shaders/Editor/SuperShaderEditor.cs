using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MobilePipeline.Shaders.Editor
{
    #if UNITY_EDITOR
    public class SuperShaderEditor : ShaderGUI
    {
        private enum DrawMode
        {
            Opaque,
            Transparent
        }

        private enum LightMode
        {
            Lambert,
            HalfLambert,
            BlinnPhong
        }

        private LightMode Lighting
        {
            set
            {
                SetKeywordEnabled("_LAMBERT", value == LightMode.Lambert);
                SetKeywordEnabled("_HALF_LAMBERT", value == LightMode.HalfLambert);
                SetKeywordEnabled("_BLINN_PHONG", value == LightMode.BlinnPhong);
            }
        }

        private Object[] _materials;
        private MaterialEditor _editor;
        private MaterialProperty[] _properties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            _properties = props;
            _editor = materialEditor;
            _materials = materialEditor.targets;
            if (_materials != null)
            {
                base.OnGUI(materialEditor, props);
                DrawSpecular();
                DrawGloss();
                DrawLightingSelector();
                DrawPresetSelector();
            }
        }

        private void DrawLightingSelector()
        {
            var prop = FindProperty("_LightModel", _properties);
            var mode = (LightMode)prop.floatValue;
            EditorGUI.BeginChangeCheck();
            mode = (LightMode)EditorGUILayout.Popup("Light mode", (int)mode, Constants.LightModesArray);
            if (EditorGUI.EndChangeCheck())
            {
                _editor.RegisterPropertyChangeUndo("Light mode");
                prop.floatValue = (float)mode;
                Lighting = mode;
            }

            EditorGUI.showMixedValue = false;
        }

        private void DrawPresetSelector()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Preset Selection");
            if (GUILayout.Button("Opaque"))
            {
                ApplyDrawMode(DrawMode.Opaque);
            }

            if (GUILayout.Button("Transparent"))
            {
                ApplyDrawMode(DrawMode.Transparent);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSpecular()
        {
            var prop = FindProperty("_Specular", _properties);
            prop.floatValue = _editor.RangeProperty(prop, "Specular");
        }

        private void DrawGloss()
        {
            var prop = FindProperty("_Gloss", _properties);
            prop.floatValue = _editor.RangeProperty(prop, "Gloss");
        }

        private void ApplyDrawMode(DrawMode mode)
        {
            foreach (Material material in _materials)
            {
                switch (mode)
                {
                    case DrawMode.Opaque:
                        material.SetOverrideTag("RenderType", "Geometry");
                        material.SetInt(Constants.SrcBlend, (int) BlendMode.One);
                        material.SetInt(Constants.DstBlend, (int) BlendMode.Zero);
                        material.SetInt(Constants.ZWrite, 1);
                        material.renderQueue = (int) RenderQueue.Geometry;
                        break;
                    case DrawMode.Transparent:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt(Constants.SrcBlend, (int) BlendMode.SrcAlpha);
                        material.SetInt(Constants.DstBlend, (int) BlendMode.OneMinusSrcAlpha);
                        material.SetInt(Constants.ZWrite, 0);
                        material.renderQueue = (int) RenderQueue.Transparent;
                        break;
                }
            }
        }

        private static class Constants
        {
            public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
            public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
            public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

            public static readonly string[] LightModesArray = Enum.GetNames(typeof(LightMode));
        }

        private void SetKeywordEnabled(string keyword, bool enabled)
        {
            if (enabled)
            {
                foreach (Material m in _materials)
                {
                    m.EnableKeyword(keyword);
                }
            }
            else
            {
                foreach (Material m in _materials)
                {
                    m.DisableKeyword(keyword);
                }
            }
        }
    }
    #endif
}