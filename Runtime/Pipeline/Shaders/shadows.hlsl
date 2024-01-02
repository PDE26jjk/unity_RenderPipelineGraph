#include <HLSLSupport.cginc>
#ifndef SHADOWS_INCLUDE
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_SHADOWED_OTHER_LIGHT_COUNT 16
#define MAX_CASCADE_COUNT 4

#if defined(_DIRECTIONAL_PCF3)
    #define DIRECTIONAL_FILTER_SAMPLES 4
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
    #define DIRECTIONAL_FILTER_SAMPLES 9
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
    #define DIRECTIONAL_FILTER_SAMPLES 16
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

#if defined(_OTHER_PCF3)
    #define OTHER_FILTER_SAMPLES 4
    #define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_OTHER_PCF5)
    #define OTHER_FILTER_SAMPLES 9
    #define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_OTHER_PCF7)
    #define OTHER_FILTER_SAMPLES 16
    #define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
TEXTURE2D_SHADOW(_OtherShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    float4 _ShadowAtlasSize;
    float4 _ShadowDistanceFade;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4 _CascadeData[MAX_CASCADE_COUNT];
    float4 _OtherShadowTiles[MAX_SHADOWED_OTHER_LIGHT_COUNT];
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT ];
    float4x4 _OtherShadowMatrices[MAX_SHADOWED_OTHER_LIGHT_COUNT];

    int _CascadeCount;
CBUFFER_END
float FadedShadowStrength (float distance, float scale, float fade) {
    return saturate((1.0 - distance * scale) * fade);
}
float FilterDirectionalShadow (float3 positionSTS) {
#if defined(DIRECTIONAL_FILTER_SETUP)
    float weights[DIRECTIONAL_FILTER_SAMPLES];
    float2 positions[DIRECTIONAL_FILTER_SAMPLES];
    float4 size = _ShadowAtlasSize.yyxx;
    DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);
    float shadow = 0;
    for (int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++) {
        shadow += weights[i] * SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, 
            SHADOW_SAMPLER,float3(positions[i].xy, positionSTS.z));
    }
    return shadow;
#else
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, 
            SHADOW_SAMPLER, positionSTS);
#endif
}


float SampleOtherShadowAtlas (float3 positionSTS, float3 bounds) {
	positionSTS.xy = clamp(positionSTS.xy, bounds.xy, bounds.xy + bounds.z);
    return SAMPLE_TEXTURE2D_SHADOW(
        _OtherShadowAtlas, SHADOW_SAMPLER, positionSTS
    );
}

float FilterOtherShadow (float3 positionSTS, float3 bounds) {
    #if defined(OTHER_FILTER_SETUP)
        real weights[OTHER_FILTER_SAMPLES];
        real2 positions[OTHER_FILTER_SAMPLES];
        float4 size = _ShadowAtlasSize.wwzz;
        OTHER_FILTER_SETUP(size, positionSTS.xy, weights, positions);
        float shadow = 0;
        for (int i = 0; i < OTHER_FILTER_SAMPLES; i++) {
            shadow += weights[i] * SampleOtherShadowAtlas(
                float3(positions[i].xy, positionSTS.z), bounds
            );
        }
        return shadow;
    #else
        return SampleOtherShadowAtlas(positionSTS, bounds);
    #endif
}
static const float3 pointShadowPlanes[6] = {
	float3(-1.0, 0.0, 0.0),
	float3(1.0, 0.0, 0.0),
	float3(0.0, -1.0, 0.0),
	float3(0.0, 1.0, 0.0),
	float3(0.0, 0.0, -1.0),
	float3(0.0, 0.0, 1.0)
};

float GetOtherShadow(uint lightIndex,float3 toLight,float3 positionWS, float3 positionCS, float3 normalWS, float backedShadow){ 
	bool isPoint = _OtherLightShadowData[lightIndex].z == 1.0; 
    float tileIndex = _OtherLightShadowData[lightIndex].y;
	float3 lightPlane = _OtherLightDirections[lightIndex].xyz;

    if (isPoint) { 
		float faceOffset = CubeMapFaceID(-toLight);
		tileIndex += faceOffset;
        lightPlane = pointShadowPlanes[faceOffset];
	}
    float strength = _OtherLightShadowData[lightIndex].x;
    int shadowMaskChannel = _OtherLightShadowData[lightIndex].w;
	float4 tileData = _OtherShadowTiles[tileIndex];

//if(strength <=0)return backedShadow;
	float distanceToLightPlane = dot(toLight,lightPlane );
	float3 normalBias = normalWS * (distanceToLightPlane * tileData.w);

	float4 positionSTS = mul(_OtherShadowMatrices[tileIndex], 
	            float4(positionWS + normalBias , 1.0));

    float shadow = FilterOtherShadow(positionSTS.xyz / positionSTS.w, tileData.xyz);
    return shadow;
}

float GetDirectionalShadow(uint lightIndex,float3 positionWS, float3 positionCS, float3 normalWS, float backedShadow){ 
    float strength = _DirectionalLightShadowData[lightIndex].x;
    if(strength <=0)return backedShadow;
    int i;
    float depth = -TransformWorldToView(positionWS).z;
    strength = FadedShadowStrength(
        depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y
    );
    float cascadeBlend = 1.0;
    for (i = 0; i < _CascadeCount; i++) {
        float4 sphere = _CascadeCullingSpheres[i];
        float3 toCenter = positionWS - sphere.xyz;
        float distanceSqr = dot(toCenter,toCenter);
        if (distanceSqr < sphere.w) {
            float fade = FadedShadowStrength(
                distanceSqr, _CascadeData[i].x, _ShadowDistanceFade.z
            ); 
            if (i == _CascadeCount - 1) {
                strength *= fade;
            }else{
                cascadeBlend = fade;
            }
            break;
        }
    }
    float dither = InterleavedGradientNoise(positionCS.xy, 0);
    if (i == _CascadeCount) {
        strength = 0;
    }
    #if defined(_CASCADE_BLEND_DITHER)
        else if (cascadeBlend < dither) {
            i += 1;
        }
    #endif
    #if !defined(_CASCADE_BLEND_SOFT)
        cascadeBlend = 1.0;
    #endif
    uint cascadeIndex = i;
    uint tileIndex = _DirectionalLightShadowData[lightIndex].y + cascadeIndex;
    float3 normalBias = normalWS * _CascadeData[cascadeIndex].y *_DirectionalLightShadowData[lightIndex].z;

    float3 positionSTS = mul(_DirectionalShadowMatrices[lightIndex + tileIndex], 
        float4(positionWS + normalBias , 1.0)).xyz;

    float shadow = FilterDirectionalShadow(positionSTS);
    if (cascadeBlend < 1.0) {
        normalBias = normalWS * (_CascadeData[cascadeIndex + 1].y *_DirectionalLightShadowData[lightIndex].z);
        positionSTS = mul(
            _DirectionalShadowMatrices[tileIndex + 1],
            float4(positionWS + normalBias, 1.0)
        ).xyz;
        shadow = lerp(
            FilterDirectionalShadow(positionSTS), shadow, cascadeBlend
        );
    }
    shadow = lerp(backedShadow, shadow, strength);
    #if defined(_SHADOW_MASK_ALWAYS)
    shadow  = min(shadow,backedShadow);
    #endif

    return shadow;
}


#define SHADOWS_INCLUDE
#endif