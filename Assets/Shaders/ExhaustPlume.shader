Shader "Unlit/ExhaustPlume"
{
	Properties
	{
		[HDR] _OuterFlame ("外焰颜色", color) = (1,0,0,1)
		[HDR] _InnerFlame ("内焰颜色", color) = (0,1,1,1)
		_Width ("内焰宽度", Range(1.001, 2)) = 1.1
		_Height ("内焰高度", Range(0.001, 1)) = 1
		_Size ("外焰长度", Range(0.001, 1)) = 1
	}
	SubShader
	{
		Tags 
		{ 
			"Queue" = "Transparent" 
            "RenderType" = "Transparent"
		}
		LOD 100
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "FastNoiseLite.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _OuterFlame;
			fixed4 _InnerFlame;
			float _Width;
			float _Height;
			float _Size;
			
			float2 remap_uv(float2 uv)
            {
	            return float2((uv.x - 0.5) * 2, (uv.y - 0.5) * 2);
            }
			
			float outer_fire_gradient(float2 uv)
            {
                float2 m_uv = remap_uv(uv);
            	const float x = (1 - abs(m_uv.x)) * 4;
            	const float y = 1 - uv.y / _Size;
            	return smoothstep(0, 1, x * y);
            }
			float inner_fire_gradient(float2 uv)
            {
                float2 m_uv = remap_uv(uv);
            	const float x = (1 - abs(m_uv.x)) * _Width;
            	const float y = 1 - uv.y / _Height;
            	return smoothstep(0, 1, x * y);
            }
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				const float2 uv = float2(i.uv.x, i.uv.y);
				float outer_gradient = outer_fire_gradient(uv);
				float inner_gradient = inner_fire_gradient(uv);
				
				fixed4 inner_flame = fixed4(_InnerFlame.xyz * inner_gradient, inner_gradient);
				fixed4 outer_flame = fixed4(_OuterFlame.xyz * outer_gradient, outer_gradient);
				return lerp(outer_flame, inner_flame, outer_gradient);
			}
			ENDCG
		}
	}
}