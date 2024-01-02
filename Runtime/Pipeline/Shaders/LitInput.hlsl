#ifndef LIT_INPUT_INCLUDE
#define LIT_INPUT_INCLUDE

CBUFFER_START(UnityPerMaterial)
	float4 _BaseMap_ST;
	float _NormalScale;
	float4 _BaseColor;
	float4 _EmissionColor;
	float _Roughness;
	float _Metallic;
CBUFFER_END


#endif