Shader "MySRP/TaaUpdate"
{

    SubShader
    {
        HLSLINCLUDE
        #pragma target 2.0
        #pragma multi_compile _ DISABLE_TEXTURE2D_X_ARRAY
        #pragma multi_compile _ BLIT_SINGLE_SLICE
        #pragma multi_compile_local _ BLIT_DECODE_HDR
        #include "Input.hlsl"
        // blit dependence
        #define TEXTURE2D_X(textureName) TEXTURE2D(textureName)
        #define SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, coord2, lod)   SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
        // blit dependence end

        // Core.hlsl for XR dependencies
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off
            Name "taaUpdate"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            TEXTURE2D_X(lastFrame);
            TEXTURE2D_X(_MotionVectorMap);
            float4 Frag(Varyings input) : SV_Target {
                float2 uv= input.texcoord.xy;
                float2 motionVector = SAMPLE_TEXTURE2D_X_LOD(_MotionVectorMap, sampler_PointClamp, uv, 0).xy;
                float4 currentFrameColor = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel);
                float4 lastFrameColor = SAMPLE_TEXTURE2D_X_LOD(lastFrame, sampler_PointClamp, uv - motionVector, 0);
                float threshold = 0.5;
                if(length((currentFrameColor - lastFrameColor))>threshold) lastFrameColor = currentFrameColor;
                float4 color = lerp(currentFrameColor,lastFrameColor,0.5);
                // float4 color = float4(motionVector,0,0);
                // float4 color = float4(currentFrameColor - lastFrameColor);
                return color;
                 
            }
            ENDHLSL
        }
    }
    Fallback Off
}