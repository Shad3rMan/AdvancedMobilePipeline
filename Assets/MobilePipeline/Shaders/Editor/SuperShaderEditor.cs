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
                DrawPresetSelector();
                DrawBaseProperties(materialEditor, props);
                DrawMainTextureBlock();
                DrawLightBlock();
            }
        }

        private void DrawBaseProperties(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Base properties");
            base.OnGUI(materialEditor, props);
            GUILayout.EndVertical();
        }

        private void DrawMainTextureBlock()
        {
            EditorGUI.BeginChangeCheck();
            var hasTex = FindProperty("_HasMainTex", _properties);
            var prop = FindProperty("_MainTex", _properties);
            GUILayout.BeginVertical(GUI.skin.box);
            hasTex.floatValue = EditorGUILayout.Toggle(prop.displayName + " enabled", hasTex.floatValue > 0) ? 1 : 0;
            if (hasTex.floatValue > 0)
            {
                EditorGUILayout.Space();
                _editor.TextureProperty(prop, prop.displayName);
            }

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                SetKeywordEnabled("_MAIN_TEX", hasTex.floatValue > 0);
            }
        }

        private void DrawLightBlock()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.BeginChangeCheck();
            var lit = FindProperty("_HasLighting", _properties);
            lit.floatValue = EditorGUILayout.Toggle(lit.displayName + " enabled", lit.floatValue > 0) ? 1 : 0;
            if (lit.floatValue > 0)
            {
                EditorGUILayout.Space();
                DrawLightingSelector();
                DrawAmbientBlock();
            }

            if (EditorGUI.EndChangeCheck())
            {
                SetKeywordEnabled("_LIT", lit.floatValue > 0);
                if (lit.floatValue < 0.001f)
                {
                    SetKeywordEnabled("_LAMBERT", false);
                    SetKeywordEnabled("_HALF_LAMBERT", false);
                    SetKeywordEnabled("_BLINN_PHONG", false);
                    SetKeywordEnabled("_AMBIENT", false);
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawAmbientBlock()
        {
            EditorGUI.BeginChangeCheck();
            var hasAmbient = FindProperty("_HasAmbientTex", _properties);
            var prop = FindProperty("_AmbientTex", _properties);
            EditorGUILayout.Space();
            hasAmbient.floatValue =
                EditorGUILayout.Toggle(prop.displayName + " enabled", hasAmbient.floatValue > 0) ? 1 : 0;
            if (hasAmbient.floatValue > 0)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                _editor.TextureProperty(prop, prop.displayName);
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                SetKeywordEnabled("_AMBIENT", hasAmbient.floatValue > 0);
            }
        }

        private void DrawLightingSelector()
        {
            EditorGUI.BeginChangeCheck();
            var prop = FindProperty("_LightModel", _properties);
            var mode = (LightMode) prop.floatValue;
            mode = (LightMode) EditorGUILayout.Popup("Light mode", (int) mode, Constants.LightModesArray);

            if (mode == LightMode.BlinnPhong)
            {
                DrawSpecular();
                DrawGloss();
            }

            if (EditorGUI.EndChangeCheck())
            {
                _editor.RegisterPropertyChangeUndo("Light mode");
                prop.floatValue = (float) mode;
                Lighting = mode;
            }
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

        private bool GetKeywordEnabled(string keyword)
        {
            bool enabled = false;
            foreach (Material m in _materials)
            {
                if (m.IsKeywordEnabled(keyword))
                {
                    enabled = true;
                }
            }

            return enabled;
        }
    }
#endif
}