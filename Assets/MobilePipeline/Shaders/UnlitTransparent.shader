Shader "MobilePipeline/Unlit Transparent"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            
            #include "ShaderLibrary/Unlit.hlsl"
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            
            ENDHLSL
        }
    }
}
