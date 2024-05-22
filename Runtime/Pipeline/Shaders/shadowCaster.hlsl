#ifndef SHADOW_CASTER_INCLUDE
#define SHADOW_CASTER_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "./input.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

struct Attributes
{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 texcoord : TEXCOORD;
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float3 positionWS : VAR_POSITION;
	float3 normalWS : VAR_NORMAL;
	float2 uv : TEXCOORD;
};

#include "./litInput.hlsl"

bool _ShadowPancaking;

VaryingsLit vert(Attributes IN) {
	VaryingsLit OUT;
	OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
	OUT.uv = TRANSFORM_TEX(IN.texcoord, _BaseMap);
	OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
	if (_ShadowPancaking) {
		#if UNITY_REVERSED_Z
		OUT.positionCS.z =
		min(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
		#else
	OUT.positionCS.z =
		max(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
		#endif
	}
	return OUT;
}

void frag(VaryingsLit IN) {

}

#endif