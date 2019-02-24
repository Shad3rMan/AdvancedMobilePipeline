Shader "MobilePipeline/Lit"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderQueue"="Geometry"}
        LOD 100
        ZWrite On
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            
            #include "ShaderLibrary/Lit.hlsl"
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            
            #pragma target 3.5
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            
            ENDHLSL
        }
    }
}
