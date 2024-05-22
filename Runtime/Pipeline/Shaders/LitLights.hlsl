#ifndef LIT_LightS_INCLUDE
#define LIT_LightS_INCLUDE

#include "./BRDF.hlsl"

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

#include "./shadows.hlsl"
float3 LitDirectionalLight(float3 normalRaw,BRDF_INPUT brdfInput) {
	float3 color = 0;
	for (int i = 0; i < _DirectionalLightCount; i++) {
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
		float shadow = GetDirectionalShadow(i, brdfInput.positionWS, brdfInput.positionCS.xyz, normalRaw, gi_shadows);
		#endif
		//////////////////////////////////////////////
		// BRDF
		//////////////////////////////////////////////
		float3 light = _DirectionalLightDirections[i].xyz;
		brdfInput.lightDir = light;
		brdfInput.lightColor = _DirectionalLightColors[i].rgb * shadow;

		#if _BRDF_catlikeCoding
		color += catlikeCodingBRDF(brdfInput);
		#elif _BRDF_Unity
		color += BRDF_Unity(brdfInput);
		#endif
	}
	return color;
}
float3 LitOtherLight(float3 normalRaw,BRDF_INPUT brdfInput) {
	float3 color =0;
	//////////////////////////////////////////////
	// Other Light
	//////////////////////////////////////////////
	brdfInput.giDiffuse = 0;
	brdfInput.giSpecular = 0;
	for (int i = 0; i < _OtherLightCount; i++) {
		float3 toLight = _OtherLightPositions[i].xyz - brdfInput.positionWS;
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
		float shadow = GetOtherShadow(i, toLight, brdfInput.positionWS, brdfInput.positionCS.xyz, normalRaw, gi_shadows);
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
		brdfInput.lightColor = _OtherLightColors[i].rgb * spotAttenuation * rangeAttenuation / distanceSqr * shadow;

		#if _BRDF_catlikeCoding
		color += catlikeCodingBRDF(brdfInput);
		#elif _BRDF_Unity
		color += BRDF_Unity(brdfInput);
		#endif
	}
	return color;
}

#endif