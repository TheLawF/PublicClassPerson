#include <HLSLSupport.cginc>
#include <UnityShaderVariables.cginc>

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