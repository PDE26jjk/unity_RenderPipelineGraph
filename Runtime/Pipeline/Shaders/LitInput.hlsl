#ifndef LIT_INPUT_INCLUDE
#define LIT_INPUT_INCLUDE
#include "./GI.hlsl"
CBUFFER_START(UnityPerMaterial)
	float4 _BaseMap_ST;
	float _NormalScale;
	float4 _BaseColor;
	float4 _EmissionColor;
	float _Roughness;
	float _Metallic;
CBUFFER_END

struct AttributesLit
{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 texcoord : TEXCOORD0;
	float4 tangentOS : TANGENT;
	GI_ATTRIBUTE_DATA
};

struct VaryingsLit
{
	float4 positionCS : SV_POSITION;
	float4 positionCSRaw : VAR_POSITION2;
	float3 positionWS : VAR_POSITION;
	float3 normalWS : VAR_NORMAL;
	float2 uv : TEXCOORD;
	float4 tangentWS : VAR_TANGENT;
	GI_VARYINGS_DATA
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_EmissionMap);

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);

#endif