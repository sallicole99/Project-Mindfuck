Shader "Legacy Shaders/Simplified/Sprites/Overlay"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (1,1,1,1)
        _BlendFactor ("Blend Factor", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Lighting Off
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _BlendFactor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 spriteColor = i.color;

                fixed4 baseColor = texColor * spriteColor;
                baseColor.rgb *= unity_AmbientSky.rgb;

                fixed4 overlay = texColor * _Color * spriteColor;

                fixed4 finalColor = lerp(baseColor, overlay, _BlendFactor);

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}