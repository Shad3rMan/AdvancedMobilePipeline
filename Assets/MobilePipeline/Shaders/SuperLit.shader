Shader "MobilePipeline/SuperLit"
{
    Properties
    {
        [HideInInspector]_Color ("Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_MainTex("Albedo & Alpha", 2D) = "white" {}
        [HideInInspector] _Cull ("Cull", Float) = 2
		[HideInInspector] _Lit ("Lit", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        ZTest LEqual
        Cull [_Cull]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        Pass
        {
            HLSLPROGRAM
            
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            
            #include "ShaderLibrary/Lit.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            ENDHLSL
        }
    }
    
    CustomEditor "MobilePipeline.Shaders.Editor.SuperShaderEditor"
}
