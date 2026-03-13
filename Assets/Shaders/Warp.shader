Shader "Fictoshader/Warp"
{
	Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Focus ("焦点位置", Vector) = (0.5, 0.5, 0, 0)
        _ApexAngle ("顶点角度", float) = 60.0
        _WarpStrength ("扭曲强度", Range(0, 1)) = 0.3
        _Transition ("过渡平滑度", Range(0.1, 4)) = 0.5
	    _Rotation("旋转角度", float) = 0
    }
    
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "ShaderUtils.hlsl"
            
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
            float4 _Focus;
            float _ApexAngle, _WarpStrength, _Transition, _Rotation;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            // 圆锥面变形函数
            float2 coneDeform(float2 uv, float2 focus, float apexAngle, float warp, float transition) {
                // 转换为以焦点为中心的坐标
                float2 d = uv - focus.xy;
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
                float2 result = focus.xy + projectedUV * abs(warp);
                
                // 混合原始UV和变形UV
                return lerp(uv, result, abs(warp));
            }
            
            fixed4 frag (v2f i) : SV_Target {
                float2 rot = rotateUV(i.uv, _Rotation);
                float2 warpedUV = coneDeform(rot, _Focus.xy, SinePeriod(_ApexAngle, 65, 10, 76), _WarpStrength, _Transition);
                
                // 添加边缘过渡
                float2 edgeDist = abs(warpedUV - 0.5) * 2.0;
                float edgeFade = 1.0 - smoothstep(0.8, 2., max(edgeDist.x, edgeDist.y));
                
                fixed4 col = tex2D(_MainTex, warpedUV);
                col.rgb *= edgeFade;
                
                return col;
            }
            ENDCG
        }
    }
}