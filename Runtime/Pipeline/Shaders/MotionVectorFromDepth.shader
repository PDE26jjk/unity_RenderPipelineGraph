Shader "MySRP/MotionVectorFromDepth"
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
            Name "Nearest"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings input) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord.xy;
                float depth = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, _BlitMipLevel).x;
                float3 posWS = ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);

                float4 posCS = mul(_NonJitteredViewProjMatrix, float4(posWS.xyz, 1.0));
                float4 prevPosCS = mul(_PrevViewProjMatrix, float4(posWS.xyz, 1.0));

                float2 posNDC = posCS.xy * rcp(posCS.w);
                float2 prevPosNDC = prevPosCS.xy * rcp(prevPosCS.w);
                float2 velocity = (posNDC - prevPosNDC) * 0.5;
                #if UNITY_UV_STARTS_AT_TOP
                    velocity.y = -velocity.y;
                #endif
                return float4(velocity, 0, 0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}