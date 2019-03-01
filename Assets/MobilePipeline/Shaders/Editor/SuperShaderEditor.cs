using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobilePipeline.Shaders.Editor
{
    public class SuperShaderEditor : ShaderGUI
    {
        private enum BlendMode
        {
            Opaque,
            Transparent
        }
        
        private MaterialEditor _materialEditor;
        private MaterialProperty _blendMode;
        private MaterialProperty _albedoMap;
        private MaterialProperty _albedoColor;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            base.OnGUI(materialEditor, props);
            FindProperties(props);
            _materialEditor = materialEditor;
            var material = materialEditor.target as Material;
            if (material != null)
            {
                EditorGUI.BeginChangeCheck();
                {
                    SetupMaterialWithBlendMode(material, (BlendMode) material.GetFloat(Styles.Lit));
                    BlendModePopup();
                    DoAlbedoArea(material);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
            }
            
            EditorGUILayout.Space();
            _materialEditor.EnableInstancingField();
            _materialEditor.DoubleSidedGIField();
        }

        private void DoAlbedoArea(Material material)
        {
            _materialEditor.TexturePropertySingleLine(Styles.AlbedoText, _albedoMap, _albedoColor);
        }

        private void BlendModePopup()
        {
            EditorGUI.showMixedValue = _blendMode.hasMixedValue;
            var mode = (BlendMode)_blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.RenderingMode, (int)mode, Styles.BlendNames);
            if (EditorGUI.EndChangeCheck())
            {
                _materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                _blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        private void FindProperties(MaterialProperty[] props)
        {
            _blendMode = FindProperty("_Lit", props);
            _albedoMap = FindProperty("_MainTex", props);
            _albedoColor = FindProperty("_Color", props);
        }

        private static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "Geometry");
                    material.SetInt(Styles.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(Styles.DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt(Styles.ZWrite, 1);
                    material.renderQueue = (int)RenderQueue.Geometry;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt(Styles.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(Styles.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Styles.ZWrite, 1);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }
        
        private static class Styles
        {
            public static readonly int Lit = Shader.PropertyToID("_Lit");
            public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
            public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
            public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
            public static readonly GUIContent AlbedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A)");
            public static readonly string[] BlendNames = Enum.GetNames(typeof(BlendMode));
            public const string RenderingMode = "Rendering Mode";
        }
    }
}