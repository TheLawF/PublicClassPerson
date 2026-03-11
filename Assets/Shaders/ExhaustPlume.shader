Shader "Unlit/ExhaustPlume"
{
	Properties
	{
		[HDR] _OuterFlame ("外焰颜色", color) = (1,0,0,1)
		[HDR] _InnerFlame ("内焰颜色", color) = (0,1,1,1)
		_InnerWidth ("内焰宽度", Range(0, 2)) = 0.5
		_InnerLength ("内焰长度", Range(0, 1)) = 0.5
		_OuterWidth ("外焰宽度", Range(2, 4)) = 2
		_OuterLength ("外焰长度", Range(0, 1)) = 1
		
		_TotalLength ("总长度", Range(0, 1)) = 1
		_Offset ("位移", float) = 0
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
			#include "ShaderUtils.hlsl"

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
			float _InnerWidth;
			float _InnerLength;
			float _OuterWidth;
			float _OuterLength;
			float _TotalLength;
			float _Offset;
			
			float2 remap_uv(float2 uv)
            {
	            return float2((uv.x - 0.5) * 2, (uv.y - 0.5) * 2);
            }

			float voronoi(float2 uv)
            {
                fnl_state noise = fnlCreateState();
                noise.noise_type = FNL_NOISE_CELLULAR;
	            noise.frequency = 1;
                noise.cellular_jitter_mod = 1;
                noise.cellular_return_type = FNL_CELLULAR_RETURN_TYPE_DISTANCE2SUB;
                noise.cellular_distance_func = FNL_CELLULAR_DISTANCE_EUCLIDEAN;
                return fnlGetNoise2D(noise, uv.x, uv.y);
            }
			
			float outer_mask(float2 uv)
            {
                float2 m_uv = remap_uv(uv);
            	const float x = (1 - abs(m_uv.x)) * _OuterWidth;
            	const float y = (1 - uv.y / _OuterLength) * _TotalLength;
            	return smoothstep(0, 1, x * y);
            }
			float inner_mask(float2 uv)
            {
                float2 m_uv = remap_uv(uv);
            	const float x = (1 - abs(m_uv.x)) * _InnerWidth;
            	const float y = (1 - uv.y / (_InnerLength * 0.9)) * _TotalLength;
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
				float2 uv = float2(i.uv.x, i.uv.y);
				const float2 cell1_uv = float2(i.uv.x * 4, i.uv.y / 8 + _Offset);
				
				// 分别计算内外焰遮罩
				float outer = outer_mask(uv);
				float inner = inner_mask(uv);
				
				float gray = voronoi(cell1_uv);
				
				// 计算内外焰的过渡区域
				float transition_area = smoothstep(0.0, 1, inner);
				float inner_only_mask = saturate(inner - outer * 0.5);
				float outer_only_mask = outer * (1.0 - transition_area * 0.7);
				
				// 计算颜色
				fixed4 inner_color = fixed4(_InnerFlame.xyz, 1.0) * inner_only_mask;
				fixed4 outer_color = fixed4(_OuterFlame.xyz, 1.0) * outer_only_mask;
				fixed4 turbulence = fixed4(_OuterFlame.xyz, 1) * outer_only_mask * gray;
				
				// 组合颜色（内焰在最上层）
				fixed4 col = fixed4(0, 0, 0, 0);
				
				// 先绘制外焰
				col.xyz = outer_color.xyz;
				col.a = outer_color.a;
				
				// 再叠加内焰
				add_layer(col, inner_color, UTIL_LAYER_BLEND_MODE_ALPHA);
				add_layer(col, turbulence, UTIL_LAYER_BLEND_MODE_ALPHA);
				
				col.a = col.a * 2;
				return col;
			}
			ENDCG
		}
	}
}