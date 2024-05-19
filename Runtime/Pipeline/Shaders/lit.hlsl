#ifndef LIT_INCLUDE
#define LIT_INCLUDE

#include "./input.hlsl"
#include "./litInput.hlsl"
#include "./BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "./GI.hlsl"

struct Attributes
{
	float4 positionOS   : POSITION;
	float3 normalOS :NORMAL;
	float2 texcoord   : TEXCOORD0;
	float4 tangentOS : TANGENT;
	GI_ATTRIBUTE_DATA
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float3 positionWS : VAR_POSITION;
	float3 normalWS : VAR_NORMAL;
	float2 uv : TEXCOORD;
	float4 tangentWS : VAR_TANGENT;
	GI_VARYINGS_DATA
};


#define MAX_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_OTHER_LIGHT_COUNT 64

CBUFFER_START(_Light)
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];	
	float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
	float3 _WorldSpaceCameraPos;
	int _DirectionalLightCount;

	float4 _OtherLightColors[MAX_OTHER_LIGHT_COUNT];
	float4 _OtherLightPositions[MAX_OTHER_LIGHT_COUNT];
	float4 _OtherLightDirections[MAX_OTHER_LIGHT_COUNT];
	float4 _OtherLightSpotAngles[MAX_OTHER_LIGHT_COUNT];
	float4 _OtherLightShadowData[MAX_OTHER_LIGHT_COUNT];
	int _OtherLightCount;
CBUFFER_END

TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
TEXTURE2D(_EmissionMap); 


#include "./shadows.hlsl"

float3 DecodeNormal (float4 sample, float scale) {
	#if defined(UNITY_NO_DXT5nm)
		return UnpackNormalRGB(sample, scale);
	#else
		return UnpackNormalmapRGorAG(sample, scale);
	#endif
}
TEXTURE2D(_NormalMap);
float3 GetNormalTS (float2 baseUV) {
	float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, baseUV);
	float scale = _NormalScale;
	float3 normal = DecodeNormal(map, scale);
	return normal;
}   
float3 NormalTangentToWorld (float3 normalTS, float3 normalWS, float4 tangentWS) {
	float3x3 tangentToWorld =
		CreateTangentToWorld(normalWS, tangentWS.xyz, tangentWS.w);
	return TransformTangentToWorld(normalTS, tangentToWorld);
}

Varyings vertLit (Attributes IN)
{
	Varyings OUT;
	OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
	OUT.uv = TRANSFORM_TEX(IN.texcoord,_BaseMap);
	OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
	OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
	TRANSFER_GI_DATA(IN, OUT)
	return OUT;
}


void ClipLOD (float2 positionCS, float fade) {
	#if defined(LOD_FADE_CROSSFADE)
	float dither = InterleavedGradientNoise(positionCS.xy, 0);
		clip(fade + (fade < 0.0 ? dither : -dither));
	#endif
}
struct FragGBufferOutput
{
	half4 GBuffer0 : SV_Target0;
	half4 GBuffer1 : SV_Target1;
	half4 GBuffer2 : SV_Target2;
	// half4 GBuffer3 : SV_Target3; // Camera color attachment
};
FragGBufferOutput fragGBuffer(Varyings IN) {
	FragGBufferOutput OUT;
	OUT.GBuffer0 = half4(1,0,0,1);
	OUT.GBuffer1 = half4(1,1,0,1);
	OUT.GBuffer2 = half4(1,1,1,1);
	// OUT.GBuffer3 = 0;
	return OUT;
}
float4 fragLit (Varyings IN) : SV_TARGET {
	// return 1;
#ifdef _WATER
	float3 pos = IN.positionWS;
	float2 xz = float2(pos.x,pos.z);
	float dis = length(xz);
	pos.y += _A * sin(dis * _w + _Time.y * _phi);
	float term1 = _A * _w * cos(dis * _w + _Time.y * _phi)/dis;
	float3 binormal = normalize(float3(0, term1* pos.x,1));
	float3 tangent = normalize(float3(1, term1 * pos.z,0));
	float3 normal = cross(binormal,tangent);
	IN.normalWS = cross(binormal,tangent);
	IN.positionWS = pos;
#else
	//ClipLOD(IN.positionCS.xy, unity_LODFade.x); 
	IN.normalWS = normalize(IN.normalWS);
#endif
	float3 normalRaw = IN.normalWS;
	IN.normalWS = normalize(NormalTangentToWorld(GetNormalTS(IN.uv), IN.normalWS, IN.tangentWS)); 
	float3 color = (float3)0;
	//////////////////////////////////////////////
	// GI
	//////////////////////////////////////////////
	float2 lightMapUV = GI_FRAGMENT_DATA(IN);
	float3 gi_diffuse = SampleLightMap(lightMapUV).rgb + SampleLightProbe(IN.positionWS,IN.normalWS).rgb;

	float3 view = normalize(_WorldSpaceCameraPos - IN.positionWS);
	float3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb *_BaseColor.rgb;
	BRDF_INPUT brdfInput;
	brdfInput.viewDir = view;
	brdfInput.giDiffuse = gi_diffuse;
	brdfInput.giSpecular = SampleEnvironment(view,IN.normalWS,IN.positionWS,_Roughness);
	brdfInput.normal = IN.normalWS;
	brdfInput.smoothness = 1-_Roughness; // perceptualroughness
	brdfInput.oneMinusReflectivity = max(0.02,1-_Metallic);
	brdfInput.diffColor = baseColor * (lerp(1.0 - 0.220916301, 0, _Metallic));
	brdfInput.specColor = lerp(0.220916301,baseColor,_Metallic);

	int i;
	for(i = 0; i < _DirectionalLightCount;i++){
	float gi_shadows = 1.;

	//////////////////////////////////////////////
	// shadow
	//////////////////////////////////////////////
  #if !defined(_RECEIVE_SHADOWS)
	float shadow = 1;
  #else
	 #if defined(_SHADOW_MASK_DISTANCE) || defined(_SHADOW_MASK_ALWAYS)
		int shadowMaskChannel = _DirectionalLightShadowData[i].w;
		if(shadowMaskChannel >=0)
		gi_shadows = SampleBakedShadows(lightMapUV,IN.positionWS)[shadowMaskChannel];
	#endif
   // normal map does not affect shadow
	float shadow = GetDirectionalShadow(i,IN.positionWS,IN.positionCS.xyz,normalRaw,gi_shadows); 
  #endif 
	//////////////////////////////////////////////
	// BRDF
	//////////////////////////////////////////////
	float3 view = normalize(_WorldSpaceCameraPos - IN.positionWS);
	float3 light = _DirectionalLightDirections[i].xyz;
	brdfInput.lightDir = light;
	brdfInput.lightColor = _DirectionalLightColors[i].rgb * shadow;

  #if _BRDF_catlikeCoding
	color += catlikeCodingBRDF(brdfInput);
  #elif _BRDF_Unity
	color += BRDF_Unity(brdfInput);
  #endif
  
	}

	//////////////////////////////////////////////
	// Other Light
	//////////////////////////////////////////////
	brdfInput.giDiffuse = 0;
	brdfInput.giSpecular = 0;
	for (i = 0; i < _OtherLightCount; i++) {
	float3 toLight = _OtherLightPositions[i].xyz - IN.positionWS;
	//////////////////////////////////////////////
	// shadow
	//////////////////////////////////////////////
  #if !defined(_RECEIVE_SHADOWS)
	float shadow = 1;
  #else
	float gi_shadows = 1.;
	#if defined(_SHADOW_MASK_DISTANCE) || defined(_SHADOW_MASK_ALWAYS)
	float strength = _OtherLightShadowData[i].x;
	int shadowMaskChannel = _OtherLightShadowData[i].w;
	if(shadowMaskChannel >=0)
		gi_shadows = SampleBakedShadows(lightMapUV,IN.positionWS)[shadowMaskChannel];
	#endif
	// normal map does not affect shadow
	float shadow = GetOtherShadow(i,toLight,IN.positionWS,IN.positionCS.xyz,normalRaw,gi_shadows); 
  #endif

	//////////////////////////////////////////////
	// BRDF
	//////////////////////////////////////////////
	float distanceSqr = max(dot(toLight, toLight), 0.001);
	float rangeAttenuation = sqr(
		saturate(1.0 - sqr(distanceSqr * _OtherLightPositions[i].w))
	);
	// rangeAttenuation = lerp(0,1,distanceSqr < _OtherLightPositions[i].w);
	brdfInput.lightDir = normalize(toLight);
	float4 spotAngles = _OtherLightSpotAngles[i];
	float spotAttenuation = sqr(
		saturate(dot(_OtherLightDirections[i].xyz, brdfInput.lightDir) *
		spotAngles.x + spotAngles.y)
	);
	brdfInput.lightColor = _OtherLightColors[i].rgb*spotAttenuation * rangeAttenuation /distanceSqr * shadow;

  #if _BRDF_catlikeCoding
	color += catlikeCodingBRDF(brdfInput);
  #elif _BRDF_Unity
	color += BRDF_Unity(brdfInput);
  #endif
	}
	float3 emission =_EmissionColor.rgb * SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, IN.uv).rgb ; 
	//color += gi_diffuse;
	//return shadow;
	//return float4(gi_diffuse,1);
	return float4(color + emission,1);
}   
#endif