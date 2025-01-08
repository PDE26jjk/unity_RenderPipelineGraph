#ifndef LIT_INCLUDE
#define LIT_INCLUDE
// #include "./input.hlsl"
#include "./litInput.hlsl"
#include "./LitLights.hlsl"
// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
// #include "./GI.hlsl" 
#include "./BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

float3 DecodeNormal(float4 sample, float scale) {
	#if defined(UNITY_NO_DXT5nm)
		return UnpackNormalRGB(sample, scale);
	#else
	return UnpackNormalmapRGorAG(sample, scale);
	#endif
}
float3 GetNormalTS(float2 baseUV) {
	#if defined(RayTracePass)
	float4 map = SAMPLE_TEXTURE2D_LOD(_NormalMap, sampler_BaseMap, baseUV, 0);
	#else
	float4 map = SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, baseUV);
	#endif
	float scale = _NormalScale;
	float3 normal = DecodeNormal(map, scale);
	return normal;
}
float3 NormalTangentToWorld(float3 normalTS, float3 normalWS, float4 tangentWS) {
	float3x3 tangentToWorld =
	CreateTangentToWorld(normalWS, tangentWS.xyz, tangentWS.w);
	return TransformTangentToWorld(normalTS, tangentToWorld);
}

VaryingsLit vertLit(AttributesLit IN) {
	VaryingsLit OUT;
	OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
	OUT.positionCSRaw = OUT.positionCS;
	OUT.uv = TRANSFORM_TEX(IN.texcoord, _BaseMap);
	OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
	OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
	TRANSFER_GI_DATA(IN, OUT)
	return OUT;
}


void ClipLOD(float2 positionCS, float fade) {
	#if defined(LOD_FADE_CROSSFADE)
	float dither = InterleavedGradientNoise(positionCS.xy, 0);
		clip(fade + (fade < 0.0 ? dither : -dither));
	#endif
}
struct FragGBufferOutput
{
	half4 GBuffer0 : SV_Target0; // base color
	half4 GBuffer1 : SV_Target1; // mix
	half4 GBuffer2 : SV_Target2; // normalWS
	half4 GBuffer3 : SV_Target3; // Camera color attachment
	float GBuffer4 : SV_Target4; // Depth map
};
float4 _ScreenParams;
FragGBufferOutput fragGBuffer(VaryingsLit IN) {
	FragGBufferOutput OUT;
	float4 baseColorTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
	IN.normalWS = normalize(NormalTangentToWorld(GetNormalTS(IN.uv), IN.normalWS, IN.tangentWS));
	float3 normal = IN.normalWS;
	OUT.GBuffer0 = half4(baseColorTex.rgb * _BaseColor.rgb, 1);
	float3 posWS = ComputeWorldSpacePosition(IN.positionCS.xy, IN.positionCS.z,UNITY_MATRIX_I_VP);

	// float4 x= mul(UNITY_MATRIX_VP,float4(IN.positionWS,1));
	float4 v = mul(unity_MatrixV, float4(IN.positionWS, 1));
	// v.xyz = TransformWorldToView(IN.positionWS);
	float4 x = mul(UNITY_MATRIX_P, v);
	x = TransformWorldToHClip(IN.positionWS);

	float4 cs = float4(IN.positionCS.xy / _ScreenParams.xy * 2 - 1, IN.positionCS.zw);
	OUT.GBuffer4 = cs.z;
	#if !UNITY_REVERSED_Z
	cs.z = lerp(UNITY_NEAR_CLIP_VALUE, 1, cs.z);
	#endif
	#if UNITY_UV_STARTS_AT_TOP
	cs.y = -cs.y;
	#endif

	float4 cs2 = float4(IN.positionCSRaw.xyz / IN.positionCSRaw.w, IN.positionCSRaw.w);
	// OUT.GBuffer0 = abs(cs-cs2);
	// OUT.GBuffer0 = -v.z;
	float4 cs_ = cs;
	cs_.xyz *= cs.w;
	posWS = mul(UNITY_MATRIX_I_VP, cs_);
	// OUT.GBuffer0 = half4(IN.positionWS,1);
	// OUT.GBuffer0 = half4(posWS,1);

	OUT.GBuffer1 = half4(_Roughness, _Metallic, 1, baseColorTex.a);
	OUT.GBuffer2 = float4(normalize(IN.normalWS), 1);
	OUT.GBuffer3 = 0;
	return OUT;
}
#define SHADOW_SAMPLER sampler_linear_clamp_compare
float4 fragLit(VaryingsLit IN) : SV_TARGET {
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
	float3 gi_diffuse = SampleLightMap(lightMapUV).rgb + SampleLightProbe(IN.positionWS, IN.normalWS).rgb;

	float3 view = normalize(_WorldSpaceCameraPos - IN.positionWS);
	float3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
	BRDF_INPUT brdfInput;
	brdfInput.posMaxErr = 0;
	brdfInput.viewDir = view;
	brdfInput.positionCS = IN.positionCS;
	brdfInput.positionWS = IN.positionWS;
	brdfInput.giDiffuse = gi_diffuse;
	brdfInput.giSpecular = SampleEnvironment(view, IN.normalWS, IN.positionWS, _Roughness);
	brdfInput.normal = IN.normalWS;
	brdfInput.smoothness = 1 - _Roughness; // perceptualroughness
	brdfInput.oneMinusReflectivity = max(0.02, 1 - _Metallic);
	brdfInput.diffColor = baseColor * (lerp(1.0 - 0.220916301, 0, _Metallic));
	brdfInput.specColor = lerp(0.220916301, baseColor, _Metallic);

	color += LitDirectionalLight(normalRaw, brdfInput);
	color += LitOtherLight(normalRaw, brdfInput);

	float3 emission = _EmissionColor.rgb * SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, IN.uv).rgb;
	return float4(color + emission, 1);
}
#endif