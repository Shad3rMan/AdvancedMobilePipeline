#ifndef LIT_HLSL
#define LIT_HLSL

#include "UnityShaderUtilities.cginc"
#include "UnityInstancing.cginc"
#include "UnityLightingCommon.cginc"
#include "Library.hlsl"

CBUFFER_START(UnityPerMaterial)
    half4 _MainTex_ST;
    half4 _AmbientTex_ST;
    half4 _EmissionTex_ST;
    half4 _PlanarTex_ST;
    half4 _NormalMap_ST;
    half4 _PlanarMask;
    half _Specular;
    half _Gloss;
    half _Emission;
    half _BumpScale;
CBUFFER_END

sampler2D   _MainTex;
sampler2D _AmbientTex;
sampler2D _EmissionTex;
sampler2D _PlanarTex;
sampler2D _NormalMap;

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

float3 DiffuseLight (float3 normal, float3 worldPos)
{
    float3 lightColor = _LightColor0.rgb;
    float4 lightPositionOrDirection = _WorldSpaceLightPos0;

    float3 lightVector = lightPositionOrDirection.xyz - worldPos * lightPositionOrDirection.w;
    float3 lightDirection = normalize(lightVector);

    float diffuse = 1;
    #if defined(_HALF_LAMBERT)
    diffuse = HalfLambert(normal, lightVector);
    #elif defined(_LAMBERT)
    diffuse = Lambert(normal, lightVector);
    #elif defined(_BLINN_PHONG)
    float3 viewDir = _WorldSpaceCameraPos.xyz - worldPos;
    diffuse = BlinnPhong(normal, lightVector, lightColor, viewDir, _Specular, _Gloss);
    #else
    diffuse = 1;
    #endif

    return diffuse * lightColor;
}

struct VertexInput
{
    float4 pos : POSITION;
    float2 uv0 : TEXCOORD0;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput 
{
    float4 clipPos : SV_POSITION;
    float2 uv0 : UV0;
    float3 normal : NORMAL;
#if defined(_LIT)
    float3 worldPos : WORLD_POS;
#endif
#if defined(_PLANAR)
    float4 localPos : LOCAL_POS;
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
    output.normal = mul((float3x3)UNITY_MATRIX_M, input.normal);

#if defined(_LIT)
    output.worldPos = worldPos.xyz;
#endif

#if defined(_PLANAR)
    output.localPos = (input.pos) + 0.5;
#endif

    return output;
}

float4 LitPassFragment (VertexOutput input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 albedo = float4(1, 1, 1, 1);

#if defined(_MAIN_TEX)
    albedo = tex2D(_MainTex, input.uv0);
#endif

    float3 color = albedo.rgb;

#if defined(_AMBIENT)
    float4 ambient = tex2D(_AmbientTex, input.uv0);
#endif

#if defined(_PLANAR)
    half4 planar = half4(0, 0, 0, 0);
    float3 weights = abs(input.normal);
    weights = weights / (weights.x + weights.y + weights.z);
    
#ifdef _PLANAR_X
    float2 uv_front = TRANSFORM_TEX(input.localPos.xy, _PlanarTex);
    half4 planar_xy = tex2D(_PlanarTex, uv_front) * weights.z;
    color = lerp(color, planar_xy, planar_xy.a);
#endif

#ifdef _PLANAR_Y
    float2 uv_side = TRANSFORM_TEX(input.localPos.zy, _PlanarTex);
    half4 planar_zy = tex2D(_PlanarTex, uv_side) * weights.x;
    color = lerp(color, planar_zy, planar_zy.a);
#endif
    
#ifdef _PLANAR_Z
    float2 uv_top = TRANSFORM_TEX(input.localPos.xz, _PlanarTex);
    half4 planar_xz = tex2D(_PlanarTex, uv_top) * weights.y;
    color = lerp(color, planar_xz, planar_xz.a);
#endif
#endif

#ifdef _EMISSION
    float4 emission = tex2D(_EmissionTex, input.uv0);
#endif
    color *= UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);

#if defined(_LIT)

    fixed3 normal = input.normal;
#if defined(_NORMALMAP)
    normal = UnpackScaleNormal(tex2D(_NormalMap, input.uv0), _BumpScale).xzy;
    normal = normalize(normal);
#endif
    float3 diffuseLight = DiffuseLight(normal, input.worldPos);

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
