Shader "Unlit/Flame"
{
	Properties
	{
		[HDR] _Color ("火焰颜色", color) = (1,1,1,1)
		_Offset ("Y轴位移", float) = 100
		_Speed ("火焰上升速度", float) = 10
		_CellDensity ("细胞密度", Range(2, 10)) = 5
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
			
			#include "FastNoiseLite.hlsl"
			#include "UnityCG.cginc"

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
			fixed4 _Color;
			float _Speed;
			float _Offset;
			float _CellDensity;
			
			float voronoi(float2 uv)
            {
                fnl_state noise = fnlCreateState();
	            noise.frequency = _CellDensity;
            	noise.fractal_type = FNL_FRACTAL_PINGPONG;
            	noise.lacunarity = 2;
            	noise.octaves = 2;
                noise.noise_type = FNL_NOISE_CELLULAR;
                noise.cellular_jitter_mod = 1;
                noise.cellular_return_type = FNL_CELLULAR_RETURN_TYPE_DISTANCE;
                noise.cellular_distance_func = FNL_CELLULAR_DISTANCE_EUCLIDEAN;
                return fnlGetNoise2D(noise, uv.x, uv.y);
            }
			
			float2 remap_uv(float2 uv)
            {
	            return float2((uv.x - 0.5) * 2, (uv.y - 0.5) * 2);
            }
			
			float outer_fire_gradient(float2 uv)
            {
                float2 m_uv = remap_uv(uv);
            	const float x = 1 - abs(m_uv.x);
            	const float y = 1 - uv.y;
            	return smoothstep(0, 1, x * y);
            }
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				const float offset = _Time.y * _Speed;
				const float2 cell1_uv = float2(i.uv.x, i.uv.y + _Offset);
				const float2 cell2_uv = float2(i.uv.x + 5.14, i.uv.y + _Offset);
				const float2 mask_uv = float2(i.uv.x, i.uv.y);

				float gray = pow(1 - (voronoi(cell1_uv) * voronoi(cell2_uv)), 2);
				float gradient = outer_fire_gradient(mask_uv);
				float result = smoothstep(0, gray, gradient);
				
				fixed4 col = fixed4(_Color.xyz * gradient * result, gradient);
				return col;
			}
			ENDCG
		}
	}
}