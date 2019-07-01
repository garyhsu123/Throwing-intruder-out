// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/Parallax Mapping"
{
	Properties
	{
		_Tint("Tint", Color) = (1, 1, 1, 1)
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(0.01, 1)) = 0.5
		_MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _NormalMap("Normals", 2D) = "bump" {}
		_BumpScale("Bump Scale", Range(0, 1)) = 1
		[NoScaleOffset] _ParallaxMap("Parallax", 2D) = "black" {}
		_ParallaxStrength("Parallax Strength", Range(0, 0.1)) = 0
		_Steps("Parallax Step",Int) = 1
	}

	CGINCLUDE
	#define BINORMAL_PER_FRAGMENT
	#define PARALLAX_STEP 40
	//#define PARALLAX_FUNCTION ParallaxRaymarching

	
	ENDCG

	SubShader
	{
		
		Pass
		{


			Tags{
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _PARALLAX_MAP

			#include "UnityStandardBRDF.cginc"

			sampler2D _MainTex, _NormalMap, _ParallaxMap;
			float4 _MainTex_ST;
			float4 _Tint;
			float _Smoothness, _Metallic;
			float _BumpScale, _ParallaxStrength;
			const int _Steps;
			struct vertexData
			{
				float3 normal : NORMAL;
				float4 position : POSITION;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};


			struct Interpolators
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD1;
				float3 tangent : TEXCOORD2;
				float3 binormal : TEXCOORD3;
				float3 worldPos : TEXCOORD4;
				
				float3 tangentViewDir : TEXCOORD5;
				//float3 height : TEXCOORD5;
			};

			float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) {
				return cross(normal, tangent.xyz) *
					(binormalSign * unity_WorldTransformParams.w);
			}


			Interpolators  vert(vertexData v)
			{
				Interpolators  o;
				o.vertex = UnityObjectToClipPos(v.position);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld,v.position);
				o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
				o.binormal = CreateBinormal(o.normal, o.tangent, v.tangent.w);
				
				float3x3 objectToTangent = float3x3(
					v.tangent.xyz,
					cross(v.normal, v.tangent.xyz) * v.tangent.w,
					v.normal
					);
				float3 localCmaeraPos = (mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1))).xyz;
				float3 ObjSpaceViewDir = localCmaeraPos - v.position;
				o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir);
				return o;
			}

			void InitNormal(inout Interpolators i) {
				float3 tangentSpaceNormal;
				tangentSpaceNormal.xy = tex2D(_NormalMap, i.uv).wy * 2 - 1;
				tangentSpaceNormal.xy *= _BumpScale;
				tangentSpaceNormal.z = sqrt(1 - saturate(dot(tangentSpaceNormal.xy, tangentSpaceNormal.xy)));
				
				

				

				i.normal = normalize(
					tangentSpaceNormal.x * i.tangent +
					tangentSpaceNormal.y * i.binormal +
					tangentSpaceNormal.z * i.normal
				);

			
			}
			float GetParallaxHeight(float2 uv) {
				return  tex2D(_ParallaxMap, uv).g ;
			}

			float2 ParallaxOffset(float2 uv, float2 viewDir) {
				float  height = GetParallaxHeight(uv);
				
				height *= _ParallaxStrength;
				return viewDir * height;
			}

			float2 ParallaxRaymarching(float2 uv, float2 viewDir) {
				

				float2 uvOffset = 0;
				float stepSize = 1.0/ PARALLAX_STEP;
				float2 uvDelta = viewDir * (stepSize * _ParallaxStrength);
				float stepHeight = 1.0;
				float surfaceHeight = GetParallaxHeight(uv);

				float2 prevUVOffset = uvOffset;
				float prevStepHeight = stepHeight;
				float prevSurfaceHeight = surfaceHeight;
				for (int i = 1; i < PARALLAX_STEP && stepHeight > surfaceHeight; i++) {


					prevUVOffset = uvOffset;
					prevStepHeight = stepHeight;
					prevSurfaceHeight = surfaceHeight;

					uvOffset -= uvDelta;
					stepHeight -= stepSize;
					surfaceHeight = GetParallaxHeight(uv + uvOffset);
					
				}
				float prevDifference = prevStepHeight - prevSurfaceHeight;
				float difference = surfaceHeight - stepHeight;
				float t = prevDifference / (prevDifference + difference);
				uvOffset = prevUVOffset - uvDelta * t;
				return uvOffset;
			}


			void doparallax(inout Interpolators i) {
				
		
				i.tangentViewDir = normalize(i.tangentViewDir);
				
				i.tangentViewDir.xy /= (i.tangentViewDir.z );

				

				float2 uvoffset;
				uvoffset = ParallaxRaymarching(i.uv, i.tangentViewDir.xy);
				i.uv += uvoffset;
				
			}

			float4 frag (Interpolators  i) : SV_Target
			{
				
				
				doparallax(i);
				//if (i.uv.x > 1.0 || i.uv.y > 1.0 || i.uv.x < 0.0 || i.uv.y < 0.0)
				//	discard;
				InitNormal(i);

				float3 albedo = tex2D(_MainTex, i.uv ).rgb * _Tint.rgb;
				float3 specularTint = albedo * _Metallic;
				float oneMinusReflectivity = 1 - _Metallic;
				albedo *= oneMinusReflectivity;

				
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 lightColor = _LightColor0.rgb;

				//float3 reflectionDir = reflect(-lightDir, i.normal); //Blinn Phong reflection model
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 halfVector = normalize(viewDir + lightDir);//Blinn Phong model

				float3 ambient = tex2D(_MainTex, i.uv).rgb * lightColor * 0.15;
				float3 specular = specularTint * lightColor * pow(DotClamped(halfVector, i.normal), _Smoothness * 100);
				float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal);


				return float4(ambient + diffuse + specular, 1);
			}




			ENDCG
		}
	}
}
