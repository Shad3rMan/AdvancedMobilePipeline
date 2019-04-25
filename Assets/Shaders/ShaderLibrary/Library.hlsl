#ifndef LIBRARY_HLSL
#define LIBRARY_HLSL

#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

half3 UnpackScaleNormal (half4 packednormal, half bumpScale) {
	#if defined(UNITY_NO_DXT5nm)
		return packednormal.xyz * 2 - 1;
	#else
		half3 normal;
		normal.xy = (packednormal.wy * 2 - 1);
		#if (SHADER_TARGET >= 30)
			// SM2.0: instruction count limitation
			// SM2.0: normal scaler is not supported
			normal.xy *= bumpScale;
		#endif
		normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
		return normal;
	#endif
}

float Lambert(float3 normal, float3 lightDir)
{
    return saturate(dot(lightDir, normal));
}

float HalfLambert(float3 normal, float3 lightDir)
{
    return pow(saturate(dot(normal, lightDir) * 0.5 + 0.5), 2);
}

float BlinnPhong(float3 normal, float3 lightDir, float3 lightColor, float3 viewDir, half specular, half gloss)
{
    half3 h = normalize (lightDir + viewDir);
    half diff = saturate(dot(lightDir, normal));
    float nh = saturate(dot(h, normal));
    float spec = pow (nh, specular * 128.0) * gloss;
    return diff + spec;
}

#endif //LIBRARY_HLSL