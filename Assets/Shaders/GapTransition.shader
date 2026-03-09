Shader "Unlit/GapTransition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = ""
        _TransitionTex ("Transition Texture", 2D) = "black" {}
        _Progress ("Progress", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

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
                fixed4 color : COLOR;  // 支持顶点颜色
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float progress : TEXCOORD1;
                fixed4 color : COLOR;  // 传递顶点颜色
            };

            float _Progress;
            
            sampler2D _MainTex;
            sampler2D _TransitionTex;
            float4 _MainTex_ST;

            float eye_curve(float2 uv, float progress)
            {
                // 计算相对于眼睛中心的UV
                // 归一化水平坐标到-1到1范围
                float x = (uv - float2(0.5, 0.5)).x / 0.495 - 1.53;
                
                float upperEyelid = abs(progress - 1) * sin(x);
                float lowerEyelid = -abs(progress - 1) * sin(x);
                
                // 转换为相对于眼睛中心的垂直坐标
                float y = uv.y - 0.5;
                
                // 检查是否在眼皮之间
                if (y > upperEyelid && y < lowerEyelid)
                {
                    return 0.0; // 眼睛睁开区域
                }
                return 1.0; // 被眼皮遮住
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.progress = _Progress;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                float eyeMask = eye_curve(i.uv, i.progress);
                
                // 如果被眼皮遮住
                if (eyeMask > 0.5)
                {
                    // 计算过渡纹理UV
                    float2 scrollUV = i.uv;
                    // float2 direction = normalize(float2(1, 1));
                    // float time = _Time.y * _ScrollSpeed;
                    //
                    // scrollUV += direction * time;
                    // scrollUV *= _TileCount;
                    scrollUV = frac(scrollUV);
                    
                    fixed4 transitionColor = tex2D(_TransitionTex, scrollUV);
                    
                    // 完全闭合时直接显示过渡纹理
                    if (i.progress >= 1.0)
                    {
                        return transitionColor;
                    }
                    
                    // 混合场景和过渡纹理
                    return lerp(color, transitionColor, eyeMask);
                }
                color.a = 0;
                return color;
            }
            ENDCG
        }
    }
}
