Shader "Unlit/VolumetricShadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		//1.ray marching && get shadow info
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 interpolatedRay:TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;			
			float _ScatteringFactor;
			float4x4 _FrustumCornorsRay;
			sampler2D _CameraDepthTexture;
			sampler2D _ShadowMapTexture;
			sampler2D _DitherMap;
			int _RayMarchingStep;
			float _MaxRayLength;
			float _VolumetricLightIntensity;
			float _VolumetricShadowIntenstiy;
			float _MinShadow;		
			float _ShadowAttenuation;

			//重映射
			float Remap(float x,float from1,float to1,float from2,float to2) {
				return (x - from1) / (to1 - from1) * (to2 - from2) + from2;
			}

			//判断该点是否在阴影
			float2 GetShadow(float3 worldPos) {
				//比较灯光空间深度
				float4 lightPos = mul(unity_WorldToShadow[0], float4(worldPos, 1));
				float shadow = UNITY_SAMPLE_DEPTH(tex2Dlod(_ShadowMapTexture, float4(lightPos.xy,0,0)));
				float depth = lightPos.z ;
				float shadowValue = step(shadow, depth);
				//阴影的衰减
				float dis = abs(depth - shadow);								
				shadowValue += clamp(Remap(dis, _ShadowAttenuation,0.1,0,1),0,1)*(1-shadowValue);
				return shadowValue;
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				//四个顶点对应的相机近裁面向量
				int index = step(0.5, v.uv.x) + step(0.5, v.uv.y)*2;

				//int index = 0;
				/*if (v.uv.x < 0.5&&v.uv.y < 0.5)
				{
					index = 0;
				}
				else if (v.uv.x > 0.5&&v.uv.y < 0.5) {
					index = 1;
				}
				else if (v.uv.x > 0.5&&v.uv.y > 0.5) {
					index = 2;
				}
				else {
					index = 3;
				}*/

				o.interpolatedRay = _FrustumCornorsRay[index];	
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//获得世界坐标
				float depthTextureValue = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv);
				float linearEyeDepth = LinearEyeDepth(depthTextureValue);
				//限制获取到的最远距离
				linearEyeDepth = clamp(linearEyeDepth,0, _MaxRayLength);
				float3 worldPos = _WorldSpaceCameraPos + linearEyeDepth * i.interpolatedRay.xyz;

				float vShadow = 1;
				float vLight = 0;
				
				float3 rayOri = _WorldSpaceCameraPos;				
				float3 rayDir = i.interpolatedRay.xyz;

				float disCam2World = length(worldPos - _WorldSpaceCameraPos);

				//dither扰动采样点
				float2 offsetUV = fmod(floor(i.vertex.xy), 4.0);
				float ditherValue = tex2D(_DitherMap, offsetUV*0.25).a;
				rayOri += ditherValue * rayDir;

				//防止背光时也产生影响
				float3 toLight = normalize(_WorldSpaceLightPos0);
				float dotLightRayDir = dot(toLight, rayDir)*0.5 + 0.5;				
				float scatteringLight = smoothstep(0.5, 1, dotLightRayDir);
								
				float3 currentPos;

				//固定的步数得到步长
				float marchStep = disCam2World / _RayMarchingStep;

				UNITY_LOOP
				for (int j = 0; j < _RayMarchingStep; j++)
				{					
					currentPos = rayOri + i.interpolatedRay.xyz * marchStep * j;
					
					float disCam2Current = length(currentPos- _WorldSpaceCameraPos);

					//对比光线是否超过了深度
					float outOfRange = step(disCam2Current, disCam2World);

					//if (disCam2World>disCam2Current)
					//{					
						float getShadow = GetShadow(currentPos);
						vShadow -= (1- getShadow) * _VolumetricShadowIntenstiy  / _RayMarchingStep * (j+3)/_RayMarchingStep * outOfRange;
						vLight += getShadow * _VolumetricLightIntensity * scatteringLight / _RayMarchingStep * (j-3)/_RayMarchingStep * outOfRange;
					//}
					//else
					//{
					//	break;
					//}
				}

				vShadow = clamp(vShadow, _MinShadow, 1);
				vLight = pow(clamp(vLight, 0, 1),_ScatteringFactor);

				float4 col = float4(vLight, vShadow, 0, 1);				
				return col;
			}
			ENDCG
		}

		//2.blur
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 uv01 : TEXCOORD1;
				float4 uv23 : TEXCOORD2;
				float4 uv45 : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 _Offsets;

			v2f vert(appdata v) {
				v2f o;
				_Offsets *= _MainTex_TexelSize.xyxy;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv01 = v.uv.xyxy + _Offsets.xyxy*float4(1, 1, -1, -1);
				o.uv23 = v.uv.xyxy + _Offsets.xyxy*float4(1, 1, -1, -1)*2.0;
				o.uv45 = v.uv.xyxy + _Offsets.xyxy*float4(1, 1, -1, -1)*3.0;
				return o;
			}

			float4 frag(v2f i) :SV_Target{
				float4 color = float4(0,0,0,0);
				color += 0.40*tex2D(_MainTex, i.uv);
				color += 0.15*tex2D(_MainTex, i.uv01.xy);
				color += 0.15*tex2D(_MainTex, i.uv01.zw);
				color += 0.10*tex2D(_MainTex, i.uv23.xy);
				color += 0.10*tex2D(_MainTex, i.uv23.zw);
				color += 0.05*tex2D(_MainTex, i.uv45.xy);
				color += 0.05*tex2D(_MainTex, i.uv45.zw);
				return color;
			}
			ENDCG
		}

		//3.combine
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _MarchingTex;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i) :SV_Target{
				float4 finalColor = 1;
				float4 ori = tex2D(_MainTex,i.uv);
				float4 marching = tex2D(_MarchingTex, i.uv);
				finalColor.rgb = clamp(ori.rgb + marching.r*_LightColor0.rgb,0,1) * marching.g;
				return finalColor;
			}
			ENDCG
		}
	}
}
