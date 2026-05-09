Shader "Legacy Shaders/Simplified/Materials/Multiple Textures"
{
    Properties
    {
        _Color0 ("BaseColor", Color) = (1,1,1,1)
        _Color1 ("SecondaryColor", Color) = (1,1,1,1)
        _MainTex ("Base", 2D) = "white" {}
        _SecondTex ("Secondary", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

    }

    SubShader
    {
        Tags { "Queue" = "alphatest" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Lambert alphatest:_Cutoff

        sampler2D _MainTex;
        sampler2D _SecondTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_SecondTex;
        };
        
        fixed4 _Color0;
        fixed4 _Color1;

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color0;
            fixed4 s = tex2D (_SecondTex, IN.uv_SecondTex) * _Color1;
            o.Albedo = lerp(c.rgb, s.rgb, s.a);
            o.Alpha = c.a + s.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}