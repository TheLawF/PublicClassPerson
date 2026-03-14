Shader "Fictoshader/JetFlame"
{
    Properties
    {
        _ColorA("渐变颜色 A", color) = (1,0,0,1)
        _ColorB("渐变颜色 B", color) = (0,1,0,1)
        _ColorC("渐变颜色 C", color) = (0,0,1,1)
        _Offset("尾焰位移", float) = 0
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
            #include "FastNoiseLite.hlsl"

            fixed4 _ColorA;
            fixed4 _ColorB;
            fixed4 _ColorC;
            float _Offset;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 gradient(float2 uv, float smooth, float2 ratio)
            {
                fixed4 col;
                if (uv.y <= ratio.x)
                {
                    return _ColorA;
                }
                if (uv.y > ratio.x && uv.y <= ratio.x + smooth)
                {
                    return lerp(_ColorA, _ColorB, uv.y);
                }
                if (uv.y <= ratio.y)
                {
                    return _ColorB;
                }
                if (uv.y > ratio.y && uv.y <= ratio.y + smooth)
                {
                    return lerp(_ColorB, _ColorC, uv.y);
                }
                return _ColorC;
            }

            float open_simplex(float2 uv)
            {
                fnl_state noise = fnlCreateState();
                noise.noise_type = FNL_NOISE_OPENSIMPLEX2;
                noise.fractal_type = FNL_FRACTAL_FBM;
                noise.frequency = .01;
                noise.lacunarity = 2;
                noise.octaves = 2;
                noise.gain = 0.5;

                return fnlGetNoise2D(noise, uv.x, uv.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = float2(i.uv.x, i.uv.y * 4 + _Offset);
                float noise = open_simplex(uv);
                fixed4 col = lerp(_ColorA, _ColorB, i.uv.y);
                return fixed4(col.xyz, 1);
            }
            ENDCG
        }
    }
}
