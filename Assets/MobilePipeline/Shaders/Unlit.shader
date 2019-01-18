Shader "MobilePipeline/Unlit"
{
    Properties
    {
		_Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
			
			#include "ShaderLibrary/Unlit.hlsl"
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma target 3.5
            #pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			
			ENDHLSL
        }
    }
}
