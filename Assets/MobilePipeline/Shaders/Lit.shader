Shader "MobilePipeline/Lit"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        ZWrite On
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            
            #include "ShaderLibrary/Lit.hlsl"
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            
            ENDHLSL
        }
    }
}
