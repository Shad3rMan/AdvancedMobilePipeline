Shader "Shad3rMan/Super Shader"
{
    Properties
    {
        [HideInInspector] _Color ("Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _HasLighting ("Lighting", Float) = 1
        [HideInInspector] _HasAmbientTex ("Ambient occlusion", Float) = 1
        [HideInInspector] _AmbientTex("Ambient Occlusion", 2D) = "white" {}
        [HideInInspector] _HasEmissionTex ("Emission", Float) = 1
        [HideInInspector] _EmissionTex("Emission", 2D) = "black" {}
        [HideInInspector] _HasNormalMap ("Normal Map", Float) = 1
        [HideInInspector] _NormalMap("Normal Map", 2D) = "bump" {}
        [HideInInspector] _HasPlanarTex ("Planar", Float) = 0
        [HideInInspector] _PlanarTex("Planar", 2D) = "white" {}
        [HideInInspector] _PlanarMask("Planar mask", Vector) = (1, 1, 1)
        [HideInInspector] _HasMainTex ("HasMainTex", Float) = 1
        [HideInInspector] _MainTex("Albedo & Alpha", 2D) = "white" {}
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCmp ("Compare Function", Float) = 0
        [HideInInspector] _LightModel ("Lighting Model", Float) = 0
        [HideInInspector] _Specular ("Specular", Range(0.01, 1)) = 1
        [HideInInspector] _Gloss ("Glossiness", Range(0, 5)) = 1
        [HideInInspector] _Emission ("Emission", Range(0, 5)) = 1
        [HideInInspector] _BumpScale ("Bump scale", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode" = "ForwardBase"}
        LOD 100
        ZTest LEqual
        Cull [_CullMode]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        Pass
        {
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilCmp]
                Pass [_StencilOp]
            }

            HLSLPROGRAM

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #pragma shader_feature _ _LIT
            #pragma shader_feature _MAIN_TEX
            #pragma shader_feature _AMBIENT
            #pragma shader_feature _EMISSION
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _PLANAR
            #pragma shader_feature _PLANAR_X
            #pragma shader_feature _PLANAR_Z
            #pragma shader_feature _PLANAR_Y
            #pragma shader_feature _ _LAMBERT _HALF_LAMBERT _BLINN_PHONG

            #include "ShaderLibrary/Lit.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            ENDHLSL
        }
    }

    CustomEditor "MobilePipeline.Shaders.Editor.SuperShaderEditor"
}
