#define TRANSFORM_TEX(tex, name) ((tex.xy) * name##_ST.xy + name##_ST.zw)
#define UNITY_MATRIX_M unity_ObjectToWorld
#include "Lighting.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
};

float Lambert(float3 normal, float3 lightDirection)
{
    return saturate(dot(normal, lightDirection));
}

float HalfLambert(float3 normal, float3 lightDirection)
{
    return pow(saturate(dot(normal, lightDirection) * 0.5 + 0.5), 2);
}

float BlinnPhong(float3 normal, float3 lightDirection, float3 worldPos, half specular, half gloss)
{
    float3 viewDir = _WorldSpaceCameraPos.xyz - worldPos;
    half3 h = normalize (lightDirection + viewDir);
    half diff = max (0, dot (normal, lightDirection));
    float nh = max (0, dot (normal, h));
    float spec = pow (nh, specular * 128.0) * gloss;

    return diff + spec;
}

float3 DiffuseLight (float3 normal, float3 worldPos)
{
    float3 lightColor = unity_LightColor[0].rgb;
    float4 lightPositionOrDirection = unity_LightPosition[0];
    float4 lightAttenuation = unity_LightAtten[0];
    float3 spotDirection = unity_SpotDirection[0].xyz;

    float3 lightVector = lightPositionOrDirection.xyz - worldPos * lightPositionOrDirection.w;
    float3 lightDirection = normalize(lightVector);

    float diffuse = 1;
    #if defined(_HALF_LAMBERT)
    diffuse = HalfLambert(normal, lightVector);
    #elif defined(_LAMBERT)
    diffuse = Lambert(normal, lightVector);
    #elif defined(_BLINN_PHONG)
    diffuse = BlinnPhong(normal, lightVector, worldPos, _Specular, _Gloss);
    #endif
    
    float rangeFade = dot(lightVector, lightVector) * lightAttenuation.x;
    rangeFade = saturate(1.0 - pow(rangeFade, 2));
    rangeFade *= rangeFade;

    float spotFade = dot(spotDirection, lightDirection);
    spotFade = saturate(spotFade * lightAttenuation.z + lightAttenuation.w);
    spotFade *= spotFade;

    float distanceSqr = max(dot(lightVector, lightVector), 0.00001);
    diffuse *= spotFade * rangeFade / distanceSqr;

    return diffuse * lightColor;
}