Shader "Legacy Shaders/Simplified/Sprites/Transparent LightMap"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _LightMap ("Lightmap (Greyscale)", 2D) = "white" {}
        [Toggle] _UseSmoothTransition ("Use Smooth Transition", Float) = 1
        _TransitionThreshold ("Lightmap Transition Threshold", Range(0.01, 1)) = 0.5
        [Toggle(_CullingOff)] _CullingOff ("Disable Culling", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 200
        Cull [_CullingOff]

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade
        #pragma target 3.0
        #pragma multi_compile_fog __ FOG_LINEAR FOG_EXP FOG_EXP2
        #pragma shader_feature _USESMOOTHTRANSITION_ON

        sampler2D _MainTex;
        sampler2D _LightMap;
        fixed4 _Color;
        float _TransitionThreshold;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_LightMap;
            fixed4 color : COLOR;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
            fixed3 mainColor = mainTex.rgb * _Color.rgb * IN.color.rgb;

            fixed lightIntensity = tex2D(_LightMap, IN.uv_LightMap).r;

            fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

            float blendFactor = 0;
            #ifdef _USESMOOTHTRANSITION_ON
                blendFactor = smoothstep(0.0, _TransitionThreshold, lightIntensity);
            #else
                blendFactor = step(_TransitionThreshold, lightIntensity);
            #endif

            fixed3 blendedLight = lerp(ambient, fixed3(lightIntensity, lightIntensity, lightIntensity), blendFactor);

            o.Albedo = mainColor;
            o.Emission = mainColor * max(blendedLight - ambient, 0);
            o.Alpha = mainTex.a * IN.color.a;
        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/Diffuse"
}