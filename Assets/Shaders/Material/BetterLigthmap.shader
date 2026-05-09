Shader "Legacy Shaders/Simplified/Materials/SychLightmap"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _LightMap ("Lightmap (RGB)", 2D) = "white" {}

        _TransitionThreshold ("Lightmap Transition Threshold", Range(0.01, 0.2)) = 0.05
        [Toggle] _UseSmoothTransition ("Use Smooth Transition", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uvMain : TEXCOORD0;
                float2 uvLightmap : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

            sampler2D _MainTex;
            sampler2D _LightMap;
            float4 _MainTex_ST;
            float4 _LightMap_ST;
            fixed4 _Color;

            float _TransitionThreshold;
            float _UseSmoothTransition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvLightmap = TRANSFORM_TEX(v.uv1, _LightMap);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex, i.uvMain) * _Color;
                fixed4 lightmap = tex2D(_LightMap, i.uvLightmap);

                fixed3 ambient = ShadeSH9(float4(i.worldNormal, 1.0));

                fixed3 diffFromWhite = 1.0 - ambient.rgb;
                float maxDiff = max(max(diffFromWhite.r, diffFromWhite.g), diffFromWhite.b);
                float blendFactor = _UseSmoothTransition > 0.5 ? smoothstep(0.0, _TransitionThreshold, maxDiff) : step(_TransitionThreshold, maxDiff);

                fixed3 finalColor = mainTex.rgb * (ambient + lightmap.rgb * blendFactor);

                fixed4 col = fixed4(finalColor, mainTex.a);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }

    FallBack "Unlit/Texture"
}