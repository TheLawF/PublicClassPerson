Shader "Unlit/Warp"
{
	Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _WarpStrength ("扭曲强度", Range(0, 1)) = 0.3
    }
    
    SubShader {
         Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WarpStrength;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            // 圆锥面变形函数
            float2 coneDeform(float2 uv, float warp) {
                // 转换为以焦点为中心的坐标
                float apexAngle = 72;
                float transition = 0.1;
                float2 d = uv - float2(0.5, 0.5);
                float r = length(d);
                
                if (r < 0.0001) return uv;
                float2 dir = d / r;
                
                // 将角度转换为弧度
                float angleRad = radians(apexAngle);
                
                // 圆锥参数
                float coneSlope = tan(angleRad * 0.5);
                
                // 计算圆锥侧面的映射
                // 平面上的点映射到圆锥侧面的3D点
                float3 conePoint;
                
                if (warp > 0) {
                    // 向外拉伸：点"爬"上圆锥侧面
                    float z = 1.0 - pow(r, transition);
                    float radiusAtZ = coneSlope * (1.0 - z);
                    conePoint = float3(dir * radiusAtZ, z);
                } else {
                    // 向内收缩：点"滑"下圆锥侧面
                    float z = pow(r, 1.0/transition);
                    float radiusAtZ = coneSlope * (1.0 - z);
                    conePoint = float3(dir * radiusAtZ, z);
                }
                
                // 从3D圆锥点投影回2D平面
                // 使用透视投影：x' = x / (1 - z)
                float2 projectedUV = conePoint.xy / (1.0 - conePoint.z + 0.001);
                
                // 应用扭曲量控制
                float2 result = float2(0.5, 0.5) + projectedUV * abs(warp);
                
                // 混合原始UV和变形UV
                return lerp(uv, result, abs(warp));
            }
            
            fixed4 frag (v2f i) : SV_Target {
                float2 warpedUV = coneDeform(i.uv, _WarpStrength);
                
                // 添加边缘过渡
                float2 edgeDist = abs(warpedUV - 0.5) * 2.0;
                float edgeFade = 1.0 - smoothstep(1, 1., max(edgeDist.x, edgeDist.y));
                
                fixed4 col = tex2D(_MainTex, warpedUV);
                col.a = edgeFade;
                
                return col;
            }
            ENDCG
        }
    }
}