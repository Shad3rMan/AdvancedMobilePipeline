Shader "MobilePipeline/Super Shader"
{
    Properties
    {
        [Toggle(_LIT)] _Lit ("Lit", Float) = 1
        [HideInInspector] _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo & Alpha", 2D) = "white" {}
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [HideInInspector] _LightModel ("Lighting Model", Float) = 0
        [HideInInspector] _Specular ("Specular", Range(0.01, 1)) = 1
        [HideInInspector] _Gloss ("Glossiness", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        ZTest LEqual
        Cull [_CullMode]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        Pass
        {
            HLSLPROGRAM

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #pragma shader_feature _LIT
            #pragma shader_feature _LAMBERT _HALF_LAMBERT _BLINN_PHONG

            #include "ShaderLibrary/Lit.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            ENDHLSL
        }
    }

    CustomEditor "MobilePipeline.Shaders.Editor.SuperShaderEditor"
}
