using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class ShadowMapHelper {

    CullingResults cullingResults;

    internal class ShadowSettings {
        public float maxDistance = 100f;

        public float distanceFade = 0.1f;

        public enum MapSize {
            _256 = 256, _512 = 512, _1024 = 1024,
            _2048 = 2048, _4096 = 4096, _8192 = 8192
        }
        public enum FilterMode {
            PCF2x2, PCF3x3, PCF5x5, PCF7x7
        }

        public struct Directional {

            public MapSize atlasSize;

            public FilterMode filter;

            [Range(1, 4)]
            public int cascadeCount;

            [Range(0f, 1f)]
            public float cascadeRatio1, cascadeRatio2, cascadeRatio3;

            [Range(0.001f, 1f)]
            public float cascadeFade;
            public Vector3 CascadeRatios =>
                new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
            public enum CascadeBlendMode {
                Hard, Soft, Dither
            }

            public CascadeBlendMode cascadeBlend;
        }
        [System.Serializable]
        public struct Other {

            public MapSize atlasSize;

            public FilterMode filter;
        }

        public Other other = new Other {
            atlasSize = MapSize._1024,
            filter = FilterMode.PCF2x2
        };

        public Directional directional = new Directional {
            atlasSize = MapSize._1024,
            filter = FilterMode.PCF2x2,
            cascadeCount = 4,
            cascadeRatio1 = 0.1f,
            cascadeRatio2 = 0.25f,
            cascadeRatio3 = 0.5f,
            cascadeFade = 0.1f,
            cascadeBlend = Directional.CascadeBlendMode.Hard

        };
        internal ShadowSettings() {
        }
    }
    ShadowSettings settings;

    const int maxShadowedDirectionalLightCount = 4, maxShadowedOtherLightCount = 16;
    const int maxCascades = 4;

    struct ShadowedDirectionalLight {
        public int visibleLightIndex;
        public float slopeScaleBias;
        public float nearPlaneOffset;

    }

    ShadowedDirectionalLight[] ShadowedDirectionalLights =
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];
    int shadowedDirLightCount, shadowedOtherLightCount;

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        otherShadowAtlasId = Shader.PropertyToID("_OtherShadowAtlas"),
        otherShadowMatricesId = Shader.PropertyToID("_OtherShadowMatrices"),
        otherShadowTilesId = Shader.PropertyToID("_OtherShadowTiles"),
        cascadeCountId = Shader.PropertyToID("_CascadeCount"),
        cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
        cascadeDataId = Shader.PropertyToID("_CascadeData"),
        shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize"),
        shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade"),
        shadowPancakingId = Shader.PropertyToID("_ShadowPancaking");

    static string[] directionalFilterKeywords = {
        "_DIRECTIONAL_PCF3",
        "_DIRECTIONAL_PCF5",
        "_DIRECTIONAL_PCF7",
    };

    static string[] otherFilterKeywords = {
        "_OTHER_PCF3",
        "_OTHER_PCF5",
        "_OTHER_PCF7",
    };

    static string[] cascadeBlendKeywords = {
        "_CASCADE_BLEND_SOFT",
        "_CASCADE_BLEND_DITHER"
    };

    static string[] shadowMaskKeywords = {
        "_SHADOW_MASK_ALWAYS",
        "_SHADOW_MASK_DISTANCE"
    };


    static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades],
        cascadeData = new Vector4[maxCascades],
        otherShadowTiles = new Vector4[maxShadowedOtherLightCount];

    static Matrix4x4[]
        dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades],
        otherShadowMatrices = new Matrix4x4[maxShadowedOtherLightCount];

    bool useShadowMask;
    Vector4 atlasSizes;
    internal void Setup(CullingResults cullingResults, ShadowSettings shadowSettings, RenderGraph renderGraph) {
        this.settings = shadowSettings;
        this.renderGraph = renderGraph;
        this.cullingResults = cullingResults;
        shadowedDirLightCount = shadowedOtherLightCount = 0;
        useShadowMask = false;
        rendererListHandles.Clear();
    }
    internal void AfterSetupLights() {
        SetupDirectionalShadows();
        SetupOtherShadows();
    }

    internal Vector4 ReserveDirectionalShadows(Light light, int visibleLightIndex, int cascadeCount) {
        if (shadowedDirLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f) {
            LightBakingOutput lightBaking = light.bakingOutput;
            float maskChannel = -1;
            if (
                lightBaking.lightmapBakeType == LightmapBakeType.Mixed &&
                lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask
            ) {
                useShadowMask = true;
                maskChannel = lightBaking.occlusionMaskChannel;
            }
            if (!cullingResults.GetShadowCasterBounds(
                visibleLightIndex, out Bounds b
            )) {
                return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
            }
            ShadowedDirectionalLights[shadowedDirLightCount] =
                new ShadowedDirectionalLight {
                    visibleLightIndex = visibleLightIndex,
                    slopeScaleBias = light.shadowBias,
                    nearPlaneOffset = light.shadowNearPlane
                };
            return new Vector4(
                light.shadowStrength,
                cascadeCount * shadowedDirLightCount++,
                light.shadowNormalBias,
                maskChannel
            );
        }
        return new Vector4(0f, 0f, 0f, -1f);
    }
    struct ShadowedOtherLight {
        public int visibleLightIndex;
        public float slopeScaleBias;
        public float normalBias;
        public bool isPoint;
    }

    ShadowedOtherLight[] shadowedOtherLights =
        new ShadowedOtherLight[maxShadowedOtherLightCount];
    internal Vector4 ReserveOtherShadows(Light light, int visibleLightIndex) {
        if (light.shadows == LightShadows.None || light.shadowStrength <= 0f) {
            return new Vector4(0f, 0f, 0f, -1f);
        }
        float maskChannel = -1f;
        LightBakingOutput lightBaking = light.bakingOutput;
        if (
            lightBaking.lightmapBakeType == LightmapBakeType.Mixed &&
            lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask
        ) {
            useShadowMask = true;
            maskChannel = lightBaking.occlusionMaskChannel;
        }
        bool isPoint = light.type == LightType.Point;
        int newLightCount = shadowedOtherLightCount + (isPoint ? 6 : 1);
        if (
            newLightCount >= maxShadowedOtherLightCount ||
            !cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)
        ) {
            return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
        }

        shadowedOtherLights[shadowedOtherLightCount] = new ShadowedOtherLight {
            visibleLightIndex = visibleLightIndex,
            slopeScaleBias = light.shadowBias,
            normalBias = light.shadowNormalBias,
            isPoint = isPoint
        };

        Vector4 data = new Vector4(
            light.shadowStrength, shadowedOtherLightCount,
            isPoint ? 1f : 0f, maskChannel);
        shadowedOtherLightCount = newLightCount;
        return data;
    }
    RasterCommandBuffer cmd;
    RenderGraph renderGraph;

    internal void SetupRenderConstant(RasterCommandBuffer cmd) {
        this.cmd = cmd;
        // RenderDirectionalShadows(cmd);

        // RenderOtherShadows(cmd);

        SetKeywords(shadowMaskKeywords, useShadowMask ?
            QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask ? 0 : 1 :
            -1
        );
        cmd.SetGlobalInt(
            cascadeCountId,
            shadowedDirLightCount > 0 ? settings.directional.cascadeCount : 0
        );
        float f = 1f - settings.directional.cascadeFade;
        cmd.SetGlobalVector(
            shadowDistanceFadeId, new Vector4(
                1f / settings.maxDistance, 1f / settings.distanceFade,
                1f / (1f - f * f)
            )
        );
        int atlasSize = (int)settings.directional.atlasSize;
        atlasSizes.x = atlasSize;
        atlasSizes.y = 1f / atlasSize;
        atlasSize = (int)settings.other.atlasSize;
        atlasSizes.z = atlasSize;
        atlasSizes.w = 1f / atlasSize;

        cmd.SetGlobalVector(shadowAtlasSizeId, atlasSizes);
        // cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
    }


    void SetupDirectionalShadows() {
        if (shadowedDirLightCount == 0) return;
        int atlasSize = (int)atlasSizes.x;
        int tiles = shadowedDirLightCount * settings.directional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int tileSize = atlasSize / split;
        for (int i = 0; i < shadowedDirLightCount; i++) {
            SetupDirectionalShadows(i, split, tileSize);
        }
        // float f = 1f - settings.directional.cascadeFade;

    }

    internal void RenderDirectionalShadows(RasterCommandBuffer cmd) {
        this.cmd = cmd;

        // cmd.SetGlobalTexture(dirShadowAtlasId, dirShadowMap);
        // cmd.SetRenderTarget(dirShadowMap);
        // cmd.ClearRenderTarget(true, false, Color.clear);

        SetKeywords(
            directionalFilterKeywords, (int)settings.directional.filter - 1
        );
        SetKeywords(
            cascadeBlendKeywords, (int)settings.directional.cascadeBlend - 1
        );
        cmd.SetGlobalFloat(shadowPancakingId, 1f);
        for (int i = 0; i < shadowedDirLightCount; i++) {
            RenderDirectionalShadows(i);
        }

        cmd.SetGlobalVectorArray(
            cascadeCullingSpheresId, cascadeCullingSpheres
        );
        cmd.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        cmd.SetGlobalVectorArray(cascadeDataId, cascadeData);
    }

    void SetupOtherShadows() {
        if (shadowedOtherLightCount == 0) return;
        int atlasSize = (int)atlasSizes.z;
        /*cmd.GetTemporaryRT(
            otherShadowAtlasId, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
        );*/
        // cmd.SetGlobalTexture(otherShadowAtlasId, otherShadowMap);
        // cmd.SetRenderTarget(
        //     otherShadowMap,
        //     RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        // );
        // cmd.ClearRenderTarget(true, false, Color.clear);

        int tiles = shadowedOtherLightCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int tileSize = atlasSize / split;

        for (int i = 0; i < shadowedOtherLightCount;) {
            if (shadowedOtherLights[i].isPoint) {
                SetupPointShadows(i, split, tileSize);
                i += 6;
            }
            else {
                SetupSpotShadows(i, split, tileSize);
                i += 1;
            }
        }
    }

    internal void RenderOtherShadows(RasterCommandBuffer cmd) {
        this.cmd = cmd;
        cmd.SetGlobalFloat(shadowPancakingId, 0f);

        for (int i = 0; i < shadowedOtherLightCount;) {
            if (shadowedOtherLights[i].isPoint) {
                RenderPointShadows(i);
                i += 6;
            }
            else {
                RenderSpotShadows(i);
                i += 1;
            }
        }

        cmd.SetGlobalMatrixArray(otherShadowMatricesId, otherShadowMatrices);
        cmd.SetGlobalVectorArray(otherShadowTilesId, otherShadowTiles);
        SetKeywords(
            otherFilterKeywords, (int)settings.other.filter - 1
        );
    }

    void SetKeywords(string[] keywords, int enabledIndex) {
        for (int i = 0; i < keywords.Length; i++) {
            if (i == enabledIndex) {
                cmd.EnableShaderKeyword(keywords[i]);
            }
            else {
                cmd.DisableShaderKeyword(keywords[i]);
            }
        }
    }
    struct ShadowRenderStruct {
        public Matrix4x4 viewMatrix;
        public Matrix4x4 projectionMatrix;
        public RendererListHandle rendererListHandle;
        public Rect viewPortRect;
    }
    static ShadowRenderStruct[]
        directionShadowRenderStructs = new ShadowRenderStruct[maxShadowedDirectionalLightCount * maxCascades];

    static ShadowRenderStruct[] otherShadowRenderStructs = new ShadowRenderStruct[maxShadowedOtherLightCount];

    static List<RendererListHandle> rendererListHandles = new();

    void SetupDirectionalShadows(int index, int split, int tileSize) {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
        var shadowSettings =
            new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        int cascadeCount = settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = settings.directional.CascadeRatios;
        float cullingFactor =
            Mathf.Max(0f, 0.8f - settings.directional.cascadeFade);
        for (int i = 0; i < cascadeCount; i++) {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, i, cascadeCount, ratios, tileSize, light.nearPlaneOffset,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData);
            ////context.CullShadowCasters(cullingResults, splitData);
            splitData.shadowCascadeBlendCullingFactor = cullingFactor;
            shadowSettings.splitData = splitData;
            if (index == 0) {
                SetCascadeData(i, splitData.cullingSphere, tileSize);
            }
            int titleIndex = tileOffset + i;
            Vector2 offset = GetTileOffset(titleIndex, split, tileSize);
            dirShadowMatrices[titleIndex] = GetShadowTransform(projectionMatrix, viewMatrix, offset, split);
            RendererListHandle shadowRendererList = renderGraph.CreateShadowRendererList(ref shadowSettings);
            directionShadowRenderStructs[titleIndex] = new ShadowRenderStruct {
                viewMatrix = viewMatrix,
                projectionMatrix = projectionMatrix,
                rendererListHandle = shadowRendererList,
                viewPortRect = GetViewPortRect(tileSize, offset)
            };
            rendererListHandles.Add(shadowRendererList);
        }

    }

    void RenderDirectionalShadows(int index) {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
        int cascadeCount = settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        for (int i = 0; i < cascadeCount; i++) {
            int titleIndex = tileOffset + i;
            var directionShadowRenderStruct = directionShadowRenderStructs[titleIndex];
            RenderShadowRenderStruct(directionShadowRenderStruct, light.slopeScaleBias);
        }
    }

    void SetupSpotShadows(int index, int split, int tileSize) {
        ShadowedOtherLight light = shadowedOtherLights[index];
        var shadowSettings =
            new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        cullingResults.ComputeSpotShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex, out Matrix4x4 viewMatrix,
            out Matrix4x4 projectionMatrix, out ShadowSplitData splitData
        );
        shadowSettings.splitData = splitData;
        float texelSize = 2f / (tileSize * projectionMatrix.m00);
        float filterSize = texelSize * ((float)settings.other.filter + 1f);
        float bias = light.normalBias * filterSize * 1.4142136f;
        Vector2 offset = GetTileOffset(index, split, tileSize);
        SetOtherTileData(index, offset, 1f / split, bias);

        otherShadowMatrices[index] = GetShadowTransform(
            projectionMatrix, viewMatrix,
            offset, split
        );
        RendererListHandle shadowRendererList = renderGraph.CreateShadowRendererList(ref shadowSettings);
        otherShadowRenderStructs[index] = new ShadowRenderStruct {
            viewMatrix = viewMatrix,
            projectionMatrix = projectionMatrix,
            rendererListHandle = shadowRendererList,
            viewPortRect = GetViewPortRect(tileSize, offset)
        };
        rendererListHandles.Add(shadowRendererList);
    }

    void SetupPointShadows(int index, int split, int tileSize) {
        ShadowedOtherLight light = shadowedOtherLights[index];
        var shadowSettings =
            new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        float texelSize = 2f / tileSize;
        float filterSize = texelSize * ((float)settings.other.filter + 1f);
        float bias = light.normalBias * filterSize * 1.4142136f;
        float tileScale = 1f / split;
        float fovBias =
            Mathf.Atan(1f + bias + filterSize) * Mathf.Rad2Deg * 2f - 90f;
        for (int i = 0; i < 6; i++) {
            cullingResults.ComputePointShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, (CubemapFace)i, fovBias,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData
            );
            viewMatrix.m11 = -viewMatrix.m11;
            viewMatrix.m12 = -viewMatrix.m12;
            viewMatrix.m13 = -viewMatrix.m13;
            shadowSettings.splitData = splitData;
            int tileIndex = index + i;
            Vector2 offset = GetTileOffset(tileIndex, split, tileSize);
            SetOtherTileData(tileIndex, offset, tileScale, bias);
            otherShadowMatrices[tileIndex] = GetShadowTransform(
                projectionMatrix, viewMatrix, offset, split
            );
            RendererListHandle shadowRendererList = renderGraph.CreateShadowRendererList(ref shadowSettings);
            otherShadowRenderStructs[tileIndex] = new ShadowRenderStruct {
                viewMatrix = viewMatrix,
                projectionMatrix = projectionMatrix,
                rendererListHandle = shadowRendererList,
                viewPortRect = GetViewPortRect(tileSize, offset)
            };
            rendererListHandles.Add(shadowRendererList);

            // cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            // cmd.SetGlobalDepthBias(0f, light.slopeScaleBias);
            // cmd.DrawRendererList(context.CreateShadowRendererList(ref shadowSettings));
            // cmd.SetGlobalDepthBias(0f, 0f);
        }
    }

    void RenderSpotShadows(int index) {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
        var shadowRenderStruct = otherShadowRenderStructs[index];
        RenderShadowRenderStruct(shadowRenderStruct, light.slopeScaleBias);
    }

    void RenderPointShadows(int index) {
        ShadowedOtherLight light = shadowedOtherLights[index];
        for (int i = 0; i < 6; i++) {
            int tileIndex = index + i;
            var shadowRenderStruct = otherShadowRenderStructs[tileIndex];
            RenderShadowRenderStruct(shadowRenderStruct, light.slopeScaleBias);
        }
    }

    void RenderShadowRenderStruct(ShadowRenderStruct shadowRenderStruct, float slopeScaleBias) {
        cmd.SetViewport(shadowRenderStruct.viewPortRect);
        cmd.SetViewProjectionMatrices(shadowRenderStruct.viewMatrix, shadowRenderStruct.projectionMatrix);
        cmd.SetGlobalDepthBias(0f, slopeScaleBias);
        cmd.DrawRendererList(shadowRenderStruct.rendererListHandle);
    }
    
    void SetOtherTileData(int index, Vector2 offset, float scale, float bias) {
        float border = atlasSizes.w * 0.5f;
        Vector4 data = Vector4.zero;
        data.x = offset.x * scale + border;
        data.y = offset.y * scale + border;
        data.z = scale - border - border;
        data.w = bias;
        otherShadowTiles[index] = data;
    }

    void SetCascadeData(int index, Vector4 cullingSphere, float tileSize) {
        float texelSize = 2f * cullingSphere.w / tileSize;
        float filterSize = texelSize * ((float)settings.directional.filter + 1f);
        cullingSphere.w -= filterSize;
        cullingSphere.w *= cullingSphere.w;
        cascadeData[index] = new Vector4(
            1f / cullingSphere.w,
            filterSize * 1.4142136f
        );

        cascadeCullingSpheres[index] = cullingSphere;
    }

    Vector2 GetTileOffset(int index, int split, float tileSize) {
        Vector2 offset = new Vector2(index % split, index / split);
        return offset;
    }
    static Rect GetViewPortRect(float tileSize, Vector2 offset) {
        return new Rect(
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
        );
    }

    static Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view, Vector2 offset, int split) {
        // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
        // apply z reversal to projection matrix. We need to do it manually here.
        if (SystemInfo.usesReversedZBuffer) {
            proj.m20 = -proj.m20;
            proj.m21 = -proj.m21;
            proj.m22 = -proj.m22;
            proj.m23 = -proj.m23;
        }

        Matrix4x4 worldToShadow = proj * view;

        var textureScaleAndBias = Matrix4x4.identity;
        float oneOver2Split = 1 / (split * 2f);
        // texture space coordinates
        // w is 1
        // u and v from [-1,1] to [offset,offset+1]/split
        textureScaleAndBias.m00 = oneOver2Split;
        textureScaleAndBias.m03 = (1 + 2 * offset.x) * oneOver2Split;
        textureScaleAndBias.m11 = oneOver2Split;
        textureScaleAndBias.m13 = (1 + 2 * offset.y) * oneOver2Split;

        // z from [-1,1] to [0,1]
        textureScaleAndBias.m22 = 0.5f;
        textureScaleAndBias.m23 = 0.5f;

        return textureScaleAndBias * worldToShadow;
    }

    public void RecordRendererLists(IBaseRenderGraphBuilder builder) {
        foreach (RendererListHandle rendererListHandle in rendererListHandles) {
            builder.UseRendererList(rendererListHandle);
        }
    }
}
