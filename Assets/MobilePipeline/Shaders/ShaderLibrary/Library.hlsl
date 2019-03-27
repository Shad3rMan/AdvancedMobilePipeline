#define TRANSFORM_TEX(tex, name) ((tex.xy) * name##_ST.xy + name##_ST.zw)
///#define UNITY_MATRIX_M unity_ObjectToWorld
//#include "Lighting.cginc"
#include "UnityInstancing.cginc"

CBUFFER_START(UnityLighting)
float4 _LightColor0;
float4x4 _LightMatrix0;
float4 _LightDirection;
float4 _LightColor;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
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
    float3 lightColor = _LightColor.rgb;//_LightColor0.rgb;
    //float4 lightPositionOrDirection = _WorldSpaceLightPos0;
    //float4 lightAttenuation = unity_4LightAtten0;
    float3 spotDirection = _LightDirection.xyz;

    //float3 lightVector = lightPositionOrDirection.xyz - worldPos * lightPositionOrDirection.w;
    //float3 lightDirection = normalize(lightVector);
    //float lengthSq = dot(lightVector, lightVector);
    //float atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0.z);
    //float spotAtt = unity_4LightAtten0.x * unity_4LightAtten0.y;
    //atten *= saturate(spotAtt);
    //float3 viewN = normalize (mul ((float3x3)UNITY_MATRIX_IT_MV, normal));
    //float diff = max (0, dot (viewN, lightVector));

    float diffuse = HalfLambert(normal, _LightDirection);
    //#if defined(_LAMBERT)
    //diffuse = Lambert(normal, _LightDirection);
    //#elif defined(_HALF_LAMBERT)
    //diffuse = HalfLambert(normal, _LightDirection);
    //#elif defined(_BLINN_PHONG)
    //diffuse = BlinnPhong(normal, _LightDirection, worldPos, _Specular, _Gloss);
    //#endif

    //float rangeFade = dot(lightVector, lightVector) * lightAttenuation.w;
    //rangeFade = saturate(1.0 - pow(rangeFade, 2));
    //rangeFade *= rangeFade;
//
    //float spotFade = dot(spotDirection, lightDirection);
    //spotFade = saturate(spotFade * lightAttenuation.x + lightAttenuation.y);
    //spotFade *= spotFade;

    //float distanceSqr = max(dot(lightVector, lightVector), 0.00001);
    //diffuse *= lengthSq / distanceSqr;//lightAttenuation.z / distanceSqr;

    return diffuse * lightColor;
}