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
        [Enum(Lambert, 0 , HalfLambert, 1, BlinnPhong, 2)] _LightModel ("Lighting Model", Float) = 0
        [HideInInspector] _Specular ("Specular", Range(0.01, 1)) = 1
        [HideInInspector] _Gloss ("Glossiness", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="ForwardBase"}
        LOD 100
        ZTest LEqual
        Cull [_CullMode]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        Pass
        {
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #pragma vertex vert
            #pragma fragment frag

            #include "ShaderLibrary/Library.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CullMode;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal);
                float4 worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0));
                o.vertex = mul(unity_MatrixVP, worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
                fixed3 diffuse = DiffuseLight(i.normal, i.worldPos);
                col.rgb *= diffuse;
                
                return col;
            }
            ENDCG
        }
    }
}
