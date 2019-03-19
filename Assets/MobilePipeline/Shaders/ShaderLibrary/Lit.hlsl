#ifndef LIT_HLSL
#define LIT_HLSL

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _AmbientTex_ST;
    float4 _EmissionTex_ST;
    half _Specular;
    half _Gloss;
    half _Emission;
CBUFFER_END

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4 unity_LightIndicesOffsetAndCount;
    float4 unity_4LightIndices0, unity_4LightIndices1;
    float3 _WorldSpaceCameraPos;
CBUFFER_END

#define MAX_VISIBLE_LIGHTS 16

#define UNITY_MATRIX_M unity_ObjectToWorld
CBUFFER_START(_LightBuffer)
    float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightDirectionsOrPositions[MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightAttenuations[MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightSpotDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_AmbientTex);
SAMPLER(sampler_AmbientTex);

TEXTURE2D(_EmissionTex);
SAMPLER(sampler_EmissionTex);

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

float3 DiffuseLight (int index, float3 normal, float3 worldPos)
{
    float3 lightColor = _VisibleLightColors[index].rgb;
    float4 lightPositionOrDirection = _VisibleLightDirectionsOrPositions[index];
    float4 lightAttenuation = _VisibleLightAttenuations[index];
    float3 spotDirection = _VisibleLightSpotDirections[index].xyz;

    float3 lightVector = lightPositionOrDirection.xyz - worldPos * lightPositionOrDirection.w;
    float3 lightDirection = normalize(lightVector);

    float diffuse = 1;
    #if defined(_HALF_LAMBERT)
    diffuse = HalfLambert(normal, lightVector);
    #elif defined(_LAMBERT)
    diffuse = Lambert(normal, lightVector);
    #elif defined(_BLINN_PHONG)
    diffuse = BlinnPhong(normal, lightVector, worldPos, _Specular, _Gloss);
    #else
    diffuse = 1;
    #endif

    float rangeFade = dot(lightVector, lightVector) * lightAttenuation.x;
    rangeFade = saturate(1.0 - pow(rangeFade, 2));
    rangeFade *= rangeFade;

    float spotFade = dot(spotDirection, lightDirection);
    spotFade = saturate(spotFade * lightAttenuation.z + lightAttenuation.w);
    spotFade *= spotFade;

    //float distanceSqr = max(dot(lightVector, lightVector), 0.00001);
    diffuse *= spotFade * rangeFade;// / distanceSqr;

    return diffuse * lightColor;
}

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

struct VertexInput
{
    float4 pos : POSITION;
    float2 uv0 : TEXCOORD0;
#if defined(_LIT)
    float3 normal : NORMAL;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput 
{
    float4 clipPos : SV_POSITION;
    float2 uv0 : TEXCOORD0;
#if defined(_LIT)
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    float3 vertexLighting : TEXCOORD3;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput LitPassVertex (VertexInput input) 
{
    VertexOutput output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
    output.clipPos = mul(unity_MatrixVP, worldPos);
    output.uv0 = TRANSFORM_TEX(input.uv0, _MainTex);

#if defined(_LIT)
    output.normal = mul((float3x3)UNITY_MATRIX_M, input.normal);
    output.worldPos = worldPos.xyz;
    output.vertexLighting = 0;
    for (int i = 4; i < min(unity_LightIndicesOffsetAndCount.y, 8); i++) {
        int lightIndex = unity_4LightIndices1[i - 4];
        output.vertexLighting += DiffuseLight(lightIndex, output.normal, output.worldPos);
    }
#endif

    return output;
}

float4 LitPassFragment (VertexOutput input, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 albedo = float4(1, 1, 1, 1);
#if defined(_MAIN_TEX)
    albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv0);
#endif

#if defined(_AMBIENT)
    float4 ambient = SAMPLE_TEXTURE2D(_AmbientTex, sampler_AmbientTex, input.uv0);
#endif

#if defined(_EMISSION)
    float4 emission = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, input.uv0);
#endif
    albedo *= UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);

    float3 color = albedo.rgb;

#if defined(_LIT)
    input.normal = normalize(input.normal);
    input.normal = IS_FRONT_VFACE(isFrontFace, input.normal, -input.normal);
    float3 diffuseLight = input.vertexLighting;
    for (int i = 0; i < min(unity_LightIndicesOffsetAndCount.y, 4); i++)
    {
        int lightIndex = unity_4LightIndices0[i];
        diffuseLight += DiffuseLight(lightIndex, input.normal, input.worldPos);
    }

    diffuseLight *= albedo.a;
    color *= diffuseLight;
#if defined(_AMBIENT)
    color *= ambient;
#endif

#if defined(_EMISSION)
    color += emission * _Emission;
#endif
#endif

    return float4(color, albedo.a);
}
#endif //LIT_HLSL
