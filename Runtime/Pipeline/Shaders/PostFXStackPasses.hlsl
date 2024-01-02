#ifndef POST_FX_PASSES_INCLUDED
#define POST_FX_PASSES_INCLUDED

// blit dependence
#define TEXTURE2D_X(textureName)                                        TEXTURE2D(textureName)
#define SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, coord2, lod)   SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
// blit dependence end

#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
 
float4 _ProjectionParams;

TEXTURE2D(_PostFXSource);
SAMPLER(sampler_linear_clamp);


float4 CopyPassFragment (Varyings input) : SV_TARGET {
	float4 color = SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_linear_clamp, input.texcoord.xy, _BlitMipLevel);
	color.r += 0;
	return color;
}
#endif