Shader "Custom/Pixelizer"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Resolution ("Resolution", Vector) = (64, 64, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct appdata members uv_MainTex)
#pragma exclude_renderers d3d11
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float2 _Resolution;

        struct appdata
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (appdata IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            float2 uv = IN.uv_MainTex;
            uv += _Resolution;
            uv = floor(uv);
            uv /= _Resolution;
            
            fixed4 c = tex2D (_MainTex, uv) * _Color;
            
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
