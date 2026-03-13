Shader "Fictoshader/NuclearExplosion" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Speed ("噪声位移速度", float) = 1.0
		_Displacement ("顶点置换强度", float) = 0.5
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#include "FastNoiseLite.hlsl"
		#pragma surface surf Standard vertex:displacement
		#pragma target 3.0

		sampler2D _MainTex;
		float _Speed;
		float _Displacement;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float2 normal: NORMAL;
		};
		
		struct Input
		{
			float2 uv_MainTex;
		};
		
		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		float open_simplex(float3 pos, float time)
        {
            fnl_state noise = fnlCreateState();
            noise.noise_type = FNL_NOISE_OPENSIMPLEX2;
            noise.fractal_type = FNL_FRACTAL_FBM;
            noise.frequency = 2;
            noise.lacunarity = 2;
            noise.octaves = 4;
			noise.gain = 0.5;

			pos.z += time;
            return fnlGetNoise3D(noise, pos.x, pos.y, pos.z);
        }
        
        // 顶点置换函数
        void displacement(inout appdata_full v)
        {
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float time = _Time.y * 3;
			
            float3 noisePos = worldPos * 2 + float3(0, 0, time);
            float noise = open_simplex(noisePos, time);
			
            v.vertex.xyz += v.normal * noise * _Displacement;
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}