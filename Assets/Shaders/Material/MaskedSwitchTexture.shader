Shader "Legacy Shaders/Simplified/Materials/Masked Switch Textures"
{
    Properties
    {
        _Color0 ("BaseColor", Color) = (1,1,1,1)
        _Color1 ("SecondaryColor", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _SecondTex ("Texture One", 2D) = "white" {}
        _SecondaryDiffrent ("Texture Two", 2D) = "white" {}
        _Mask ("Masking", 2D) = "black" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        [ToggleUI] _Swap ("Switch", Int) = 0
    }

    SubShader
    {
        Tags { "Queue" = "alphatest" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Lambert alphatest:_Cutoff
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _SecondTex;
        sampler2D _SecondaryDiffrent;
        sampler2D _Mask;

        struct Input
        {
            float2 uv_MainTex;
        };
        
        fixed4 _Color0;
        fixed4 _Color1;
        float _Swap;

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 wall = tex2D(_MainTex, IN.uv_MainTex) * _Color0;
            fixed4 door;
            if (_Swap > 0.5)
                door = tex2D(_SecondaryDiffrent, IN.uv_MainTex) * _Color1;
            else
                door = tex2D(_SecondTex, IN.uv_MainTex) * _Color1;
            
            fixed4 mask = tex2D(_Mask, IN.uv_MainTex);

            o.Albedo = lerp(wall.rgb, door.rgb, door.a);
            
            o.Alpha = wall.a * mask.a * mask.rgb + door.a * (1 - mask.a);
        }
        ENDCG
    }
    FallBack "Diffuse"
}