#ifndef BRDF_INCLUDE
#define BRDF_INCLUDE

float Pow5(float x) { return x * x * x * x * x; }
float sqr(float x) { return x * x; }

float DisneyDisfuse(float a, float NdotL, float NdotV, float LdotH) {
	float FD90 = 0.5 + 2 * a * LdotH * LdotH;
	return Pow5(lerp(1, FD90, Pow5(1 - NdotL)) * lerp(1, FD90, Pow5(1 - NdotV)));
}
float D_GGX(float a, float NdotH) {
	float d = 1. + (a - 1.) * NdotH * NdotH;
	return a / (PI * d * d + 1e-7f);
}
float SmithGGX(float NdotL, float NdotV, float roughness) {
	half a = roughness;
	half a2 = a * a;

	half lambdaV = NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
	half lambdaL = NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);

	return 0.5f / (lambdaV + lambdaL + 1e-5f);
}
float3 SchlickFresnel(float3 f0, float cosA) {
	float x = Pow5(1.0 - cosA);
	return f0 + (1.0 - f0) * x;
}
struct BRDF_INPUT
{
	float3 lightDir;
	float3 lightColor;
	float3 viewDir;
	float3 normal;
	float3 positionWS;
	float3 positionCS;
	float smoothness;
	float oneMinusReflectivity;
	float posMaxErr;
	float3 diffColor;
	float3 specColor;
	float3 giDiffuse;
	float3 giSpecular;
};
float3 FresnelLerp(float3 F0, float3 F90, float cosA) {
	half t = Pow5(1 - cosA); // ala Schlick interpoliation
	return lerp(F0, F90, t);
}

float3 mixGI(float3 diffuse, float3 specular, float nv, float roughness, BRDF_INPUT IN) {
	diffuse += IN.diffColor * IN.giDiffuse;
	float grazingTerm = saturate(IN.smoothness + (1 - IN.oneMinusReflectivity));
	specular += IN.giSpecular * FresnelLerp(IN.specColor, grazingTerm, nv) / (roughness * roughness + 1.0);

	return diffuse + specular;
}
float3 BRDF_Unity(BRDF_INPUT IN) {
	float3 halfTerm = SafeNormalize(IN.viewDir + IN.lightDir);
	float NdotL = saturate(dot(IN.normal, IN.lightDir));
	float NdotV = saturate(dot(IN.normal, IN.viewDir));
	float NdotH = saturate(dot(IN.normal, halfTerm));
	float LdotH = saturate(dot(IN.lightDir, halfTerm));
	float roughness = 1 - IN.smoothness; // perceptualroughness
	roughness *= roughness;

	float a = roughness * 0.35;
	float3 diffuse = DisneyDisfuse(a, NdotL, NdotV, LdotH) * NdotL * IN.lightColor;
	diffuse *= IN.diffColor;

	float3 F = SchlickFresnel(IN.specColor, LdotH);
	float D = D_GGX(sqr(lerp(0.002, 1, roughness)), NdotH);
	float G = min(1, SmithGGX(NdotL, NdotV, roughness));

	float3 spq = D * F * G * NdotL * PI * IN.lightColor;

	return mixGI(diffuse, spq, NdotV, roughness, IN);

}
// https://catlikecoding.com/unity/tutorials/custom-srp/directional-lights/
float3 catlikeCodingBRDF(BRDF_INPUT IN) {
	float3 h = SafeNormalize(IN.viewDir + IN.lightDir);
	float nh2 = sqr(saturate(dot(IN.normal, h)));
	float lh2 = sqr(saturate(dot(IN.lightDir, h)));
	float roughness = 1 - IN.smoothness;
	roughness *= roughness;
	float r2 = sqr(roughness);
	float d2 = sqr(nh2 * (r2 - 1.0) + 1.00001);
	float normalization = roughness * 4.0 + 2.0;
	float nl = saturate(dot(IN.normal, IN.lightDir));
	float3 spq = r2 / (d2 * max(0.1, lh2) * normalization) * nl * IN.lightColor * IN.specColor;

	float3 diffuse = nl * IN.lightColor * IN.diffColor;
	return mixGI(diffuse, spq, saturate(dot(IN.normal, IN.viewDir)), roughness, IN);
}

#endif