using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class LightingHelper {
    public static readonly LightingHelper instance = new LightingHelper();
    const int maxDirLightCount = 4, maxOtherLightCount = 64;

    static readonly int
        //dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
        //dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
        dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
        dirLightShadowDataId =
            Shader.PropertyToID("_DirectionalLightShadowData"),
        // 
        otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
        otherLightColorsId = Shader.PropertyToID("_OtherLightColors"),
        otherLightPositionsId = Shader.PropertyToID("_OtherLightPositions"),
        otherLightDirectionsId = Shader.PropertyToID("_OtherLightDirections"),
        otherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles"),
        otherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData");

    static Vector4[]
        otherLightColors = new Vector4[maxOtherLightCount],
        otherLightPositions = new Vector4[maxOtherLightCount],
        otherLightDirections = new Vector4[maxOtherLightCount],
        otherLightSpotAngles = new Vector4[maxOtherLightCount],
        otherLightShadowData = new Vector4[maxOtherLightCount];

    static Vector4[]
        dirLightColors = new Vector4[maxDirLightCount],
        dirLightDirections = new Vector4[maxDirLightCount],
        dirLightShadowData = new Vector4[maxDirLightCount];
    CullingResults cullingResults;
    ShadowMapHelper shadows = new();

    internal void Setup(CullingResults cullingResults, ShadowMapHelper.ShadowSettings shadowSettings, RenderGraph renderGraph) {
        this.cullingResults = cullingResults;
        shadows.Setup(cullingResults, shadowSettings, renderGraph);
        SetupLights(shadowSettings.directional.cascadeCount);
        shadows.AfterSetupLights();
    }

    int dirLightCount, otherLightCount;
    void SetupLights(int directionShadowCascadeCount) {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        dirLightCount = otherLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++) {
            var visibleLight = visibleLights[i];
            switch (visibleLight.lightType) {
                case LightType.Directional:
                    if (dirLightCount < maxDirLightCount) {
                        SetupDirectionalLight(dirLightCount++, i, ref visibleLight, directionShadowCascadeCount);
                    }
                    break;
                case LightType.Point:
                    if (otherLightCount < maxOtherLightCount) {
                        SetupPointLight(otherLightCount++, i, ref visibleLight);
                    }
                    break;
                case LightType.Spot:
                    if (otherLightCount < maxOtherLightCount) {
                        SetupSpotLight(otherLightCount++, i, ref visibleLight);
                    }
                    break;
            }
        }

    }
    void SetupDirectionalLight(int index, int visibleIndex, ref VisibleLight visibleLight, int cascadeCount) {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, visibleIndex, cascadeCount);
    }
    void SetupPointLight(int index, int visibleIndex, ref VisibleLight visibleLight) {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w =
            1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
        otherLightPositions[index] = position;
        otherLightSpotAngles[index] = new Vector4(0f, 1f);
        Light light = visibleLight.light;
        otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
    }

    void SetupSpotLight(int index, int visibleIndex, ref VisibleLight visibleLight) {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w =
            1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
        otherLightPositions[index] = position;
        otherLightDirections[index] =
            -visibleLight.localToWorldMatrix.GetColumn(2);
        Light light = visibleLight.light;
        float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
        float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
        float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
        otherLightSpotAngles[index] = new Vector4(
            angleRangeInv, -outerCos * angleRangeInv
        );
        otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
    }
    internal void RenderDirShadowMap(RasterCommandBuffer cmd) {
        this.shadows.RenderDirectionalShadows(cmd);
    }
    internal void RenderOtherShadowMap(RasterCommandBuffer cmd) {
        this.shadows.RenderOtherShadows(cmd);
    }

    internal void SetGlobalLightingConstant(RasterCommandBuffer cmd) {
        cmd.SetGlobalInt(dirLightCountId, dirLightCount);
        if (dirLightCount > 0) {
            cmd.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
            cmd.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
            cmd.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }

        cmd.SetGlobalInt(otherLightCountId, otherLightCount);
        if (otherLightCount > 0) {
            cmd.SetGlobalVectorArray(otherLightColorsId, otherLightColors);
            cmd.SetGlobalVectorArray(
                otherLightPositionsId, otherLightPositions
            );
            cmd.SetGlobalVectorArray(
                otherLightDirectionsId, otherLightDirections
            );
            cmd.SetGlobalVectorArray(
                otherLightSpotAnglesId, otherLightSpotAngles
            );
            cmd.SetGlobalVectorArray(
                otherLightShadowDataId, otherLightShadowData
            );
        }
        shadows.SetupRenderConstant(cmd);
    }

    public void RecordRendererLists(IBaseRenderGraphBuilder builder) {
        shadows.RecordRendererLists(builder);
    }
}
