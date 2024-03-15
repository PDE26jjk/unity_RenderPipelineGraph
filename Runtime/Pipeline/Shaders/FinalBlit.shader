// https://blog.csdn.net/wodownload2/article/details/104371831
Shader "MySRP/FinalBlit"
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
            #pragma fragment FragNearest

            // fixed4 frag(v2f i) : SV_Target {
            //     UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            //     i.texcoord -= _FinalBlitRect.xy;
            //     i.texcoord *= _FinalBlitRect.zw;
            //     if (any(i.texcoord > 1) || any(i.texcoord < 0)) clip(-1);
            //     // if (all(i.texcoord < _FinalBlitRect.zw)) { clip(-1); }
            //     return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord);
            // }
            ENDHLSL
        }
    }
    Fallback Off
}