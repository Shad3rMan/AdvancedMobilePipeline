Shader "MobilePipeline/SuperLit"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo & Alpha", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull", Float) = 2
 		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
        [HideInInspector] _DrawMode ("Draw Mode", Float) = 1
        [Toggle(_LIT)] _Lit ("Lit", Float) = 1
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
            
            #include "ShaderLibrary/Lit.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            ENDHLSL
        }
    }
    
    CustomEditor "MobilePipeline.Shaders.Editor.SuperShaderEditor"
}
