Shader "Unlit/ShaderToyFire"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
    	Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            float2 hash( float2 p )
			{
				p = float2( dot(p,float2(127.1,311.7)),
						 dot(p,float2(269.5,183.3)) );
				return -1.0 + 2.0*frac(sin(p)*43758.5453123);
			}

			float noise( in float2 p )
			{
				const float K1 = 0.366025404; // (sqrt(3)-1)/2;
				const float K2 = 0.211324865; // (3-sqrt(3))/6;
				
				float2 i = floor( p + (p.x+p.y)*K1 );
				
				float2 a = p - i + (i.x+i.y)*K2;
				float2 o = (a.x>a.y) ? float2(1.0,0.0) : float2(0.0,1.0);
				float2 b = a - o + K2;
				float2 c = a - 1.0 + 2.0*K2;
				
				float3 h = max( 0.5-float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
				
				float3 n = h*h*h*h*float3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
				
				return dot( n, float3(70.0, 70.0, 70.0) );
			}

			float fbm(float2 uv)
			{
				float2x2 m = float2x2( 1.6,  1.2, -1.2,  1.6 );
				float f = 0.5 * noise(uv); uv = mul(m, uv);
				f += 0.25 * noise( uv ); uv = mul(uv, m);
				f += 0.125 * noise( uv ); uv = mul(uv, m);
				f += 0.0625 * noise( uv ); uv = mul(uv, m);
				f = 0.5 + 0.5*f;
				return f;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
				float2 q = uv;
				float strength = floor(q.x+5.);
				float T3 = max(3.,1.25*strength)* _Time;
				q.x = frac(uv.x) - 0.5;
            	q.y -= .25;
				float n = fbm(strength*q - float2(0,T3));
				// 修改1：调整噪声影响范围，让火焰在顶部也有影响
			    float c = 1. - 12. * pow(max(0., length(q * float2(1.8 + q.y * 1.5, .75)) - n * max(0.1, q.y + 0.5)), 1.1);
			    
			    // 修改2：放宽垂直衰减限制，让火焰能延伸到顶部
			    float c1 = n * c * (1.5 - 0.5 * pow(uv.y, 2.));  // 降低指数和系数
			    
			    c1 = clamp(c1, 0., 3.);  // 提高上限
				c1=clamp(c1,0.,2.);

				float3 col = float3(1.5*c1, 1.5*c1*c1*c1, c1*c1*c1*c1*c1*c1);
				
				float a = c * (1.-pow(uv.y,1.));
				return fixed4( lerp(float3(0., 0., 0.),col,a), 1.0);
            }
            ENDCG
        }
    }
}
