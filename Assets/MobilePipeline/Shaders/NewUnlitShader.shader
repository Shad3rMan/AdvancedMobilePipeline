Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        [Toggle(_LIT)] _Lit ("Lit", Float) = 1
        [HideInInspector] _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo & Alpha", 2D) = "white" {}
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [HideInInspector] _LightModel ("Lighting Model", Float) = 0
        [HideInInspector] _Specular ("Specular", Range(0.01, 1)) = 1
        [HideInInspector] _Gloss ("Glossiness", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="Vertex"}
        LOD 100
        ZTest LEqual
        Cull [_CullMode]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "ShaderLibrary/Library.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CullMode;

            v2f vert (appdata v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = mul((float3x3)UNITY_MATRIX_M, v.normal);
                float4 worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0));
                o.vertex = mul(unity_MatrixVP, worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                fixed3 diffuse = DiffuseLight(i.normal, i.worldPos);
                
                return fixed4(col.rgb * diffuse, col.a);
            }
            ENDCG
        }
    }
    
    CustomEditor "MobilePipeline.Shaders.Editor.SuperShaderEditor"
}
