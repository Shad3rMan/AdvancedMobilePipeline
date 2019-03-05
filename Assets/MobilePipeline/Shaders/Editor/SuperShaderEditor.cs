using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobilePipeline.Shaders.Editor
{
    public class SuperShaderEditor : ShaderGUI
    {
        private enum DrawMode
        {
            Opaque,
            Transparent
        }

        private MaterialEditor _materialEditor;
        private MaterialProperty _drawMode;
        private MaterialProperty _albedoMap;
        private MaterialProperty _cullMode;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props);
            _materialEditor = materialEditor;
            
            var material = materialEditor.target as Material;
            if (material != null)
            {
                EditorGUI.BeginChangeCheck();
                {
                    SetupMaterialWithBlendMode(material, (DrawMode) material.GetFloat(Styles.DrawMode));
                    BlendModePopup();
                    CullModePopup();
                    DoAlbedoArea();
                }
                if (EditorGUI.EndChangeCheck())
                {
                }
            }

            EditorGUILayout.Space();
            _materialEditor.EnableInstancingField();
            _materialEditor.DoubleSidedGIField();
        }

        private void DoAlbedoArea()
        {
            _materialEditor.TextureProperty(_albedoMap, "Albedo");
        }

        private void BlendModePopup()
        {
            EditorGUI.showMixedValue = _drawMode.hasMixedValue;
            var mode = (DrawMode) _drawMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (DrawMode) EditorGUILayout.Popup(Styles.DrawModeText, (int) mode, Styles.DrawModeNames);
            if (EditorGUI.EndChangeCheck())
            {
                _materialEditor.RegisterPropertyChangeUndo("Draw Mode");
                _drawMode.floatValue = (float) mode;
            }

            EditorGUI.showMixedValue = false;
        }

        private void CullModePopup()
        {
            EditorGUI.showMixedValue = _cullMode.hasMixedValue;
            var mode = (CullMode) _cullMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (CullMode) EditorGUILayout.Popup(Styles.CullModeText, (int) mode, Styles.CullNames);
            if (EditorGUI.EndChangeCheck())
            {
                _materialEditor.RegisterPropertyChangeUndo("Cull Mode");
                _cullMode.floatValue = (float) mode;
            }

            EditorGUI.showMixedValue = false;
        }

        private void FindProperties(MaterialProperty[] props)
        {
            _drawMode = FindProperty("_DrawMode", props);
            _albedoMap = FindProperty("_MainTex", props);
            _cullMode = FindProperty("_CullMode", props);
        }

        private static void SetupMaterialWithBlendMode(Material material, DrawMode drawMode)
        {
            switch (drawMode)
            {
                case DrawMode.Opaque:
                    material.SetOverrideTag("RenderType", "Geometry");
                    material.SetInt(Styles.SrcBlend, (int) BlendMode.One);
                    material.SetInt(Styles.DstBlend, (int) BlendMode.Zero);
                    material.SetInt(Styles.ZWrite, 1);
                    material.renderQueue = (int) RenderQueue.Geometry;
                    break;
                case DrawMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt(Styles.SrcBlend, (int) BlendMode.One);
                    material.SetInt(Styles.DstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Styles.ZWrite, 1);
                    material.renderQueue = (int) RenderQueue.Transparent;
                    break;
            }
        }

        private static class Styles
        {
            public static readonly int DrawMode = Shader.PropertyToID("_DrawMode");
            public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
            public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
            public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

            public static readonly string[] DrawModeNames = Enum.GetNames(typeof(DrawMode));
            public static readonly string[] CullNames = Enum.GetNames(typeof(CullMode));
            public const string DrawModeText = "Rendering Mode";
            public const string CullModeText = "Cull Mode";
        }
    }
}