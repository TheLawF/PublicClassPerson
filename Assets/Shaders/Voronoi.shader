Shader "Fictoshader/PlasmaBeam"
{
    Properties
    {
        [HDR] _Color ("光束颜色", Color) = (1,1,1,1)
        _Offset ("Y轴位移", float) = 100
        _Rotation ("UV 旋转角度", Range(0,360)) = 0
        _CellDensity ("细胞数量", Range(2, 10)) = 5
        _Luminosity ("发光强度", float) = 4
        _Threshold ("透明阈值", Range(0, 1)) = 0.08
        _Cutoff ("裁剪", Range(0,1))= 0
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType" = "Transparent"
        }
        LOD 100
        Cull Off
        ZWrite Off
        ZTest LEqual
        ColorMask RGB
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include <UnityCG.cginc>
            #include "FastNoiseLite.hlsl"

            float _Offset;
            float _Rotation;
            float _Cutoff;
            float _CellDensity;
            
            float _Luminosity;
            float _Threshold;
            fixed4 _Color;
            
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
            
            float voronoi(float2 uv)
            {
                fnl_state noise = fnlCreateState();
	            noise.frequency = _CellDensity;
                noise.noise_type = FNL_NOISE_CELLULAR;
                noise.cellular_jitter_mod = 0.8;
                noise.cellular_return_type = FNL_CELLULAR_RETURN_TYPE_DISTANCE;
                noise.cellular_distance_func = FNL_CELLULAR_DISTANCE_EUCLIDEANSQ;
        
                return fnlGetNoise2D(noise, uv.x, uv.y);
            }

            float overlay(float base, float overlay)
            {
                return  2.0 * base * overlay;
            }

            float2 rotateUV(float2 uv, float angleInDegrees, float2 center = float2(0.5, 0.5))
            {
                float angleRad = angleInDegrees * 3.14159265359 / 180.0;
                uv -= center;
                float s = sin(angleRad);
                float c = cos(angleRad);
                float2x2 rotMatrix = float2x2(c, -s, s, c);
                uv = mul(rotMatrix, uv);
                uv += center;
                return uv;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i): SV_Target
            {
                float2 uv = float2(i.uv.x + _Offset * 0.1, i.uv.y + _Offset);
                float2 uv2 = float2(i.uv.x + 1.14 + _Offset * 0.1, i.uv.y + _Offset);

                float2 rotUV1 = rotateUV(uv, _Rotation);
                float2 rotUV2 = rotateUV(uv2, _Rotation);
                
                float cellular1 = voronoi(rotUV1);
                float cellular2 = voronoi(rotUV2);
                float grayBase =  0.5 + 0.5 * cellular1;
                float grayOverlay = 0.5 + 0.5 * cellular2;
                
                fixed4 col = fixed4(_Color.xyz * grayBase * _Luminosity, grayBase.x > _Threshold ? 1 : 0);
                return col;
            }
        ENDCG
        }
    }
    FallBack "Diffuse"
}
