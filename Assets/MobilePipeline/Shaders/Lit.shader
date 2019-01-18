Shader "MobilePipeline/Lit"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
        
        Pass {
			Tags { "LightMode" = "ShadowCaster" }
			
			HLSLPROGRAM
			
			#pragma target 3.5
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			
			#include "ShaderLibrary/ShadowCaster.hlsl"
			
			ENDHLSL
		}
    }
}
