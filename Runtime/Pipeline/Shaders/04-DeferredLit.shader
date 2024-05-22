Shader "MySRP/DeferredLit"
{

    SubShader
    {
        HLSLINCLUDE
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        #pragma shader_feature _BRDF_Unity _BRDF_catlikeCoding
        #pragma shader_feature _RECEIVE_SHADOWS
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ _SHADOW_MASK_ALWAYS _SHADOW_MASK_DISTANCE
        #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
        #pragma multi_compile _ _OTHER_PCF3 _OTHER_PCF5 _OTHER_PCF7
        #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER

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
            Name "DeferredLit"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            FRAMEBUFFER_INPUT_HALF(0);
            FRAMEBUFFER_INPUT_HALF(1);
            FRAMEBUFFER_INPUT_HALF(2);
            FRAMEBUFFER_INPUT_FLOAT(3);
            float4 _ZBufferParams;
            float4 _ScaledScreenParams;
            // float3 _WorldSpaceCameraPos;
            #include "LitLights.hlsl"
            #include "GI.hlsl"

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 baseColor = LOAD_FRAMEBUFFER_INPUT(0, input.positionCS);
                half4 mixMap = LOAD_FRAMEBUFFER_INPUT(1, input.positionCS);
                half3 normalWS = LOAD_FRAMEBUFFER_INPUT(2, input.positionCS);
                normalWS = normalize(normalWS);
                float deviceDepth = LOAD_FRAMEBUFFER_INPUT(3, input.positionCS);
                #if UNITY_REVERSED_Z
                #else
                // Adjust z to match NDC for OpenGL
                deviceDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, deviceDepth);
                #endif
                // float depth = LinearEyeDepth(deviceDepth, _ZBufferParams);

                float2 ndc = input.texcoord;
                #if UNITY_UV_STARTS_AT_TOP
                ndc.y = 1 - ndc.y - 1/_ScaledScreenParams.y * 2;
                #endif

                float3 positionWS = ComputeWorldSpacePosition(ndc, deviceDepth,UNITY_MATRIX_I_VP);
                // return deviceDepth;
                // float distFromEye = depth / dot(ViewDir, cameraForward);
                // float3 depthTextureWorldPos = _WorldSpaceCameraPos + ViewDir * dist;
                // return 1- deviceDepth;
                float3 tangent = ddx(positionWS);
                float3 biTangent = -ddy(positionWS);
                float3 normalRB = cross(tangent, biTangent); // normal rebuilt from depth
                normalRB = length(normalRB) > 0.00 ? normalWS : normalize(normalRB);

                // return half4(biTangent,1);
                // return length(normalRB)>0.01;
                float3 gi_diffuse = SampleLightProbe(positionWS, normalWS).rgb;
                float metallic = mixMap.g;
                float roughness = mixMap.r;
                float3 view = normalize(_WorldSpaceCameraPos - positionWS);
                BRDF_INPUT brdfInput;
                brdfInput.viewDir = view;
                brdfInput.positionCS = input.positionCS.xyz;
                brdfInput.positionWS = positionWS;
                brdfInput.giDiffuse = gi_diffuse;
                brdfInput.giSpecular = SampleEnvironment(view, normalWS, positionWS, roughness);
                brdfInput.normal = normalWS;
                brdfInput.smoothness = 1 - roughness; // perceptualroughness
                brdfInput.oneMinusReflectivity = max(0.02, 1 - metallic);
                brdfInput.diffColor = baseColor * (lerp(1.0 - 0.220916301, 0, metallic));
                brdfInput.specColor = lerp(0.220916301, baseColor, metallic);
                // return half4(normalize(normalRB), 1);
                float3 color = 0;
                color += LitDirectionalLight(normalRB, brdfInput);
                color += LitOtherLight(normalRB, brdfInput);

                return half4(color, 1);
                // return half4(normalWS, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}