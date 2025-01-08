Shader "MySRP/02-lit"
{

    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        [HDR] _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Roughness("roughness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0
        [NoScaleOffset] _NormalMap("Normals", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0, 1)) = 1
        [NoScaleOffset] _EmissionMap("Emission", 2D) = "white" {}
        [HDR] _EmissionColor("Emission", Color) = (0.0, 0.0, 0.0, 0.0)
        [Enum(Unity,0,catlikeCoding,1)] _BRDF ("BRDF", Float) = 0
        [Toggle(_RECEIVE_SHADOWS)] _RECEIVE_SHADOWS ("Receive Shadows", Float) = 1
    }
    CustomEditor "litGUI"
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "RPG" "RenderType" = "Opaque"
        }
        Pass
        {
            Tags
            {
                "LightMode" = "MySRPMode1"
            }
            Name "Lit"
            HLSLPROGRAM
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma shader_feature _BRDF_Unity _BRDF_catlikeCoding
            #pragma shader_feature _RECEIVE_SHADOWS
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _SHADOW_MASK_ALWAYS _SHADOW_MASK_DISTANCE
            #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            #pragma multi_compile _ _OTHER_PCF3 _OTHER_PCF5 _OTHER_PCF7

            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER

            #pragma vertex vertLit
            #pragma fragment fragLit

            #include "lit.hlsl"
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "GBuffer"
            }
            Name "GBuffer"
            HLSLPROGRAM
            #pragma vertex vertLit
            #pragma fragment fragGBuffer

            #include "lit.hlsl"
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            Name "ShadowCaster"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "./ShadowCaster.hlsl"
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "Meta"
            }

            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex MetaPassVertex
            #pragma fragment MetaPassFragment
            #include "./litMeta.hlsl"
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "DXR"
            }
            Name "DXR"
            HLSLPROGRAM
            // #pragma enable_d3d11_debug_symbols
            #define RayTracePass
            #include "./RayTrace/RayTrace.hlsl"
            struct AttributeData
            {
                float2 barycentrics;
            };
            #pragma only_renderers d3d11 xboxseries ps5
            #pragma raytracing whyimhere
            [shader("closesthit")]
            void ClosestHitMain(inout Hit payload : SV_RayPayload, AttributeData attribs : SV_IntersectionAttributes) {
                payload.position = WorldRayOrigin() + WorldRayDirection() * RayTCurrent();
                payload.instanceID = InstanceID();
                payload.primitiveIndex = PrimitiveIndex();
                payload.uvBarycentrics = attribs.barycentrics;
                payload.hitDistance = RayTCurrent();
                payload.isFrontFace = (HitKind() == HIT_KIND_TRIANGLE_FRONT_FACE);
                IntersectionVertex vertex;
                GetCurrentIntersectionVertex(attribs.barycentrics, vertex);
                float4 tangentWS = float4(TransformObjectToWorldDir(vertex.tangentOS.xyz), vertex.tangentOS.w);
                float3 normalWS = TransformObjectToWorldDir(vertex.normalOS);
                payload.normal = normalize(NormalTangentToWorld(GetNormalTS(vertex.texCoord0), normalWS, tangentWS));
                float4 baseColorTex = SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, vertex.texCoord0, 0);
                payload.baseColor = float4(baseColorTex.rgb * _BaseColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}