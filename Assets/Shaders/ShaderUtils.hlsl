#include <HLSLSupport.cginc>
#include <UnityShaderVariables.cginc>

#define UTIL_LAYER_BLEND_MODE_ADD 0
#define UTIL_LAYER_BLEND_MODE_ALPHA 1
#define UTIL_LAYER_BLEND_MODE_MULTIPLY 2
#define UTIL_LAYER_BLEND_MODE_SCREEN 3
#define UTIL_LAYER_BLEND_MODE_OVERLAY 4
#define UTIL_LAYER_BLEND_MODE_HARDLIGHT 5
#define UTIL_LAYER_BLEND_MODE_SOFTLIGHT 6
#define UTIL_LAYER_BLEND_MODE_DARKEN 7
#define UTIL_LAYER_BLEND_MODE_LIGHTEN 8
#define UTIL_LAYER_BLEND_MODE_COLORDODGE 9
#define UTIL_LAYER_BLEND_MODE_COLORBURN 10
#define UTIL_LAYER_BLEND_MODE_LINEARDODGE 11
#define UTIL_LAYER_BLEND_MODE_LINEARBURN 12
#define UTIL_LAYER_BLEND_MODE_LINEARLIGHT 13
#define UTIL_LAYER_BLEND_MODE_DIFFERENCE 14
#define UTIL_LAYER_BLEND_MODE_EXCLUSION 15
#define UTIL_LAYER_BLEND_MODE_SUBTRACT 16
#define UTIL_LAYER_BLEND_MODE_DIVIDE 17
#define UTIL_LAYER_BLEND_MODE_VIVIDLIGHT 18
#define UTIL_LAYER_BLEND_MODE_PINLIGHT 19
#define UTIL_LAYER_BLEND_MODE_HARDMIX 20
#define UTIL_LAYER_BLEND_MODE_ADDITIVE 21
#define UTIL_LAYER_BLEND_MODE_ADDSUB 22
#define UTIL_LAYER_BLEND_MODE_REFLECT 23
#define UTIL_LAYER_BLEND_MODE_GLOW 24
typedef int layer_blend_mode;


fixed blend_channel(fixed bottom, fixed top, layer_blend_mode mode)
{
    switch(mode)
    {
        case UTIL_LAYER_BLEND_MODE_MULTIPLY:         // 正片叠底
            return bottom * top;
        case UTIL_LAYER_BLEND_MODE_SCREEN:          // 滤色
            return 1.0 - (1.0 - bottom) * (1.0 - top);
        case UTIL_LAYER_BLEND_MODE_OVERLAY:         // 叠加
            return bottom < 0.5 ? 2.0 * bottom * top : 1.0 - 2.0 * (1.0 - bottom) * (1.0 - top);
        case UTIL_LAYER_BLEND_MODE_HARDLIGHT:       // 强光
            return top < 0.5 ? 2.0 * bottom * top : 1.0 - 2.0 * (1.0 - bottom) * (1.0 - top);
        case UTIL_LAYER_BLEND_MODE_SOFTLIGHT:      // 柔光
        {
            fixed result = 0.0;
            if (top < 0.5)
                result = 2.0 * bottom * top + bottom * bottom * (1.0 - 2.0 * top);
            else
                result = 2.0 * bottom * (1.0 - top) + sqrt(bottom) * (2.0 * top - 1.0);
            return result;
        }
        case UTIL_LAYER_BLEND_MODE_DARKEN:          // 变暗
            return min(bottom, top);
        case UTIL_LAYER_BLEND_MODE_LIGHTEN:         // 变亮
            return max(bottom, top);
        case UTIL_LAYER_BLEND_MODE_COLORDODGE:      // 颜色减淡
            return bottom / (1.0 - top);
        case UTIL_LAYER_BLEND_MODE_COLORBURN:       // 颜色加深
            return 1.0 - (1.0 - bottom) / top;
        case UTIL_LAYER_BLEND_MODE_LINEARDODGE:     // 线性减淡(相加)
            return min(bottom + top, 1.0);
        case UTIL_LAYER_BLEND_MODE_LINEARBURN:      // 线性加深
            return bottom + top - 1.0;
        case UTIL_LAYER_BLEND_MODE_LINEARLIGHT:     // 线性光
        {
            if (top < 0.5)
                return bottom + 2.0 * top - 1.0;
            else
                return bottom + 2.0 * (top - 0.5);
        }
        case UTIL_LAYER_BLEND_MODE_DIFFERENCE:      // 差值
            return abs(bottom - top);
        case UTIL_LAYER_BLEND_MODE_EXCLUSION:       // 排除
            return bottom + top - 2.0 * bottom * top;
        case UTIL_LAYER_BLEND_MODE_SUBTRACT:        // 减去
            return bottom - top;
        case UTIL_LAYER_BLEND_MODE_DIVIDE:          // 划分
            return bottom / (top + 0.0001); // 避免除0
        case UTIL_LAYER_BLEND_MODE_VIVIDLIGHT:      // 亮光
        {
            if (top < 0.5)
            {
                if (top == 0.0) return 0.0;
                return 1.0 - (1.0 - bottom) / (2.0 * top);
            }
            else
            {
                if (top == 1.0) return 1.0;
                return bottom / (2.0 * (1.0 - top));
            }
        }
        case UTIL_LAYER_BLEND_MODE_PINLIGHT:        // 点光
        {
            if (top < 0.5)
                return min(bottom, 2.0 * top);
            return max(bottom, 2.0 * (top - 0.5));
        }
        case UTIL_LAYER_BLEND_MODE_HARDMIX:         // 实色混合
        {
            fixed result = (bottom + top) >= 1.0 ? 1.0 : 0.0;
            return result;
        }
        case UTIL_LAYER_BLEND_MODE_ADDITIVE:        // 添加
            return min(bottom + top, 1.0);
        case UTIL_LAYER_BLEND_MODE_ADDSUB:          // 相加(带负值)
            return bottom + top;
        case UTIL_LAYER_BLEND_MODE_REFLECT:         // 反射
        {
            if (top == 1.0) return 1.0;
            return min(bottom * bottom / (1.0 - top), 1.0);
        }
        case UTIL_LAYER_BLEND_MODE_GLOW:            // 发光
        {
            if (bottom == 1.0) return 1.0;
            return min(top * top / (1.0 - bottom), 1.0);
        }
        default:                                    // 默认使用Alpha混合
            return top;
    }
}

void add_layer(inout fixed4 bottom_layer, fixed4 new_layer, layer_blend_mode mode)
{
    if (mode == UTIL_LAYER_BLEND_MODE_ADD)
    {
        // ADD模式：直接相加
        bottom_layer.xyz = min(bottom_layer.xyz + new_layer.xyz, 1.0);
        bottom_layer.a = max(bottom_layer.a, new_layer.a);
    }
    else if (mode == UTIL_LAYER_BLEND_MODE_ALPHA)
    {
        bottom_layer.xyz = bottom_layer.xyz * (1.0 - new_layer.a) + new_layer.xyz * new_layer.a;
        bottom_layer.a = max(bottom_layer.a, new_layer.a);
    }
}

float2 remapUV(float2 uv)
{
    return (uv.r - 0.5) * 2;
}
//重映射
float Remap(float x,float from1,float to1,float from2,float to2) {
    return (x - from1) / (to1 - from1) * (to2 - from2) + from2;
}

float2 getVolumetricShadow(float3 worldPos, sampler2D shadowMapTexture, float shadowAttenuation)
{
    //比较灯光空间深度
    float4 lightPos = mul(unity_WorldToShadow[0], float4(worldPos, 1));
    float shadow = UNITY_SAMPLE_DEPTH(tex2Dlod(shadowMapTexture, float4(lightPos.xy,0,0)));
    float depth = lightPos.z ;
    float shadowValue = step(shadow, depth);
    //阴影的衰减
    float dis = abs(depth - shadow);								
    shadowValue += clamp(Remap(dis, shadowAttenuation,0.1,0,1),0,1)*(1-shadowValue);
    return shadowValue;
}
