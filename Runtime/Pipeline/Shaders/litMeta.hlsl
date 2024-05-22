#ifndef LIT_META_INCLUDE
#define LIT_META_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "./input.hlsl"
#include "./BRDF.hlsl" 
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl"
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
TEXTURE2D(_EmissionMap); 
CBUFFER_START(UnityPerMaterial)
	float4 _BaseMap_ST;
	float _NormalScale;
	float4 _BaseColor;
	float4 _EmissionColor;
	float _Roughness;
	float _Metallic;
CBUFFER_END

struct MetaAttributes
{
	float4 positionOS   : POSITION;
	float3 normalOS     : NORMAL;
	float2 uv0          : TEXCOORD0;
	float2 uv1          : TEXCOORD1;
	float2 uv2          : TEXCOORD2;
};

struct Varyings
{
	float4 positionCS   : SV_POSITION;
	float2 uv           : TEXCOORD0;
#ifdef EDITOR_VISUALIZATION
	float2 VizUV        : TEXCOORD1;
	float4 LightCoord   : TEXCOORD2;
#endif
};
Varyings MetaPassVertex (MetaAttributes input) {
	Varyings output = (Varyings)0;
		output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
		output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);
	#ifdef EDITOR_VISUALIZATION
		UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
	#endif
		return output;
}

float4 MetaPassFragment (Varyings IN) : SV_TARGET {
	float4 meta = 0;
	if (unity_MetaFragmentControl.x) {
		float3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
		BRDF_INPUT brdfInput;
		brdfInput.smoothness = 1-_Roughness;
		brdfInput.oneMinusReflectivity = max(0.02,1-_Metallic);
		brdfInput.diffColor = baseColor * (lerp(1.0 - 0.220916301, 0, _Metallic));
		brdfInput.specColor = lerp(0.220916301,baseColor,_Metallic);
		meta = float4(brdfInput.diffColor,1);
		meta.rgb += brdfInput.specColor.rgb * _Roughness * 0.5;
		meta.rgb = min(
			PositivePow(meta.rgb, unity_OneOverOutputBoost), unity_MaxOutputValue
		);
	}else if (unity_MetaFragmentControl.y) {
		float3 emission =_EmissionColor.rgb * SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, IN.uv).rgb; 
		meta = float4(emission, 1.0);
	}
	return meta;
}

#endif