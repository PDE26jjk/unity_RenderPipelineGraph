using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RenderPipelineGraph;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using Object = UnityEngine.Object;


public class RPGRenderer : IDisposable {
    ScriptableRenderContext context;

    Camera camera;
    CameraData cameraData;

    CommandBuffer cmd;

    BufferedRTHandleSystem bufferedRTHandleSystem;

    bool needCrateSharedResource => cameraData.needReloadGraph;
    HashSet<ResourceData> m_ResourceCreateEveryFrame = new();
    HashSet<ResourceData> m_ResourceImportEveryFrame = new();
    bool needReorderPass => cameraData.needReloadGraph;

    static Comparer<PassSortData> PassNodeComparer = Comparer<PassSortData>.Create((x, y) => {
        int compareTo = x.pos.x.CompareTo(y.pos.x);
        if (compareTo == 0) {
            compareTo = x.pos.y.CompareTo(y.pos.y);
        }
        if (compareTo == 0) return 1;
        return compareTo;
    });
    internal class PassSortData {
        public Vector2 pos; // for sorting
        public SortedSet<PassSortData> followings = new(PassNodeComparer);
        public PassNodeData passNodeData;
        public MethodInfo addRenderPassMethodInfo;
        public int refCount;
        public object[] parameters;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public ProfilingSampler profilingSampler;
#endif
    }
    List<PassSortData> passSorted = new();
    RPGAsset asset;
    RenderGraph renderGraph;


    // Data stored by the camera

    public void Render(RPGAsset asset, RenderGraph renderGraph, ScriptableRenderContext context, CameraData cameraData) {
        this.context = context;
        this.camera = cameraData.camera;
        this.cameraData = cameraData;
        this.asset = asset;
        this.renderGraph = renderGraph;
        cmd = CommandBufferPool.Get();

        cameraData.BorrowRTHandles();

        var renderGraphParameters = new RenderGraphParameters {
            commandBuffer = cmd,
            currentFrameIndex = Time.frameCount,
            // executionName = "Render RPG",
            rendererListCulling = true,
            scriptableRenderContext = context
        };

        // ExecuteBuffer();

        var testDesc = new TextureDesc(camera.pixelWidth, camera.pixelHeight) {
            colorFormat = GraphicsFormat.B8G8R8A8_SRGB,
            name = "testColor"
        };
        var testDepthDesc = new TextureDesc(camera.pixelWidth, camera.pixelHeight) {
            depthBufferBits = DepthBits.Depth32,
            name = "testDepth"
        };
        // sharedTex = renderGraph.CreateSharedTexture(testDesc);
        PrepareResources();

        cameraData.SwapAndSetReferenceSize();
        // RTHandles.SetReferenceSize(camera.pixelWidth, camera.pixelHeight);

        ReorderPasses();

        // renderGraph.nativeRenderPassesEnabled = true;
        renderGraph.BeginRecording(renderGraphParameters);

        UpdateVolumeFramework();

        Cull();

        ImportBuildInRenderTexture();

        SetupCreatedResource(renderGraph);

        SetupImportedResource(renderGraph);

        SetupGlobalTexture(cmd);

        RecordPasses(renderGraph);

        renderGraph.EndRecordingAndExecute();
        renderGraph.nativeRenderPassesEnabled = false;

        // Cleanup(); 
        Submit();
        CommandBufferPool.Release(renderGraphParameters.commandBuffer);
        cameraData.RestoreRTHandels();
    }
    void RecordPasses(RenderGraph renderGraph) {
        using var profilingScope = new ProfilingScope(ProfilingSampler.Get(RPGProfileId.RecordPasses));
        foreach (PassSortData passSortData in passSorted) {
            using var recordPassProfilingScope = new ProfilingScope(passSortData.profilingSampler);
            var passNodeData = passSortData.passNodeData;
            var pass = passNodeData.m_Pass;
            if (!pass.Valid(this.camera)) continue;
            ref object[] parameters = ref passSortData.parameters;
            parameters ??= RenderGraphUtils.MakeAddRenderPassParam(renderGraph, pass, passSortData.profilingSampler);
            using (var baseBuilder = passSortData.addRenderPassMethodInfo.Invoke(renderGraph, parameters) as IBaseRenderGraphBuilder) {
                baseBuilder.AllowPassCulling(pass.AllowPassCulling);
                baseBuilder.AllowGlobalStateModification(pass.AllowGlobalStateModification);

                ref object passData = ref parameters[1];
                switch (pass.PassType) {
                    case PassNodeType.Legacy: // TODO Legacy pass
                        break;
                    case PassNodeType.Unsafe: // TODO Unsafe pass
                        break;
                    case PassNodeType.Raster:
                    {
                        var builder = baseBuilder as IRasterRenderGraphBuilder;
                        RenderGraphUtils.SetRenderFunc(builder, pass);
                        RenderGraphUtils.LoadPassData(passNodeData, passData, builder, renderGraph, this.cameraData);
                    }
                        break;
                    case PassNodeType.Compute: // TODO Compute pass
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    void ReorderPasses() {
        using var profilingScope = new ProfilingScope(ProfilingSampler.Get(RPGProfileId.ReorderPasses));
        if (needReorderPass) {
            // needReorderPass = false;
            List<PassSortData> passSortDatas = new();
            passSorted.Clear();
            Dictionary<PassNodeData, PassSortData> node2sort = new();

            foreach (NodeData nodeData in asset.Graph.NodeList) {
                if (nodeData is PassNodeData passNodeData) {
                    PassSortData passSortData = new() {
                        pos = passNodeData.pos,
                        passNodeData = passNodeData
                    };
                    passSortDatas.Add(passSortData);
                    node2sort[passNodeData] = passSortData;
                }
            }

            PassSortData root = new();
            // Add dependence info.
            foreach (PassSortData passSortData in passSortDatas) {
                passSortData.refCount = passSortData.passNodeData.dependencies.Count;
                if (passSortData.refCount == 0) {
                    passSortData.refCount = 1;
                    root.followings.Add(passSortData);
                }
                else {
                    foreach (PassNodeData dependPassData in passSortData.passNodeData.dependencies) {
                        node2sort[dependPassData].followings.Add(passSortData);
                        // Debug.Log(dependPassData.exposedName+"-->" +  passSortData.passNodeData.exposedName);
                    }
                }
            }

            // Kahn's with DFS, because SRP NRP combines subpasses using index order for now.
            Stack<PassSortData> stack = new(root.followings);
            while (stack.Count > 0) {
                PassSortData passSortData = stack.Pop();
                while (passSortData is not null) {
                    passSortData.refCount--;
                    if (passSortData.refCount == 0) {
                        passSorted.Add(passSortData);
                    }
                    else break;
                    bool isFirst = true;
                    PassSortData next = null;
                    foreach (PassSortData sortData in passSortData.followings) {
                        if (isFirst) {
                            isFirst = false;
                            next = sortData;
                            continue;
                        }
                        stack.Push(sortData);
                    }
                    passSortData.followings.Clear();
                    passSortData = next;
                }
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var str = new StringBuilder("Pass order: ");
            str.AppendJoin(", ", passSorted.Select(t => t.passNodeData.exposedName));
            Debug.Log(str.ToString());
#endif
            foreach (PassSortData passSortData in passSorted) {
                passSortData.addRenderPassMethodInfo = RenderGraphUtils.GetAddRasterRenderPassMethodInfo(renderGraph, passSortData.passNodeData.m_Pass);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                passSortData.profilingSampler = new ProfilingSampler(passSortData.passNodeData.exposedName);
#endif
            }
        }
    }
    Dictionary<RPGCullingDesc, CullingResults> m_CullingResultsMap = new();
    List<RendererListData> m_RendererListDatas = new();
    List<CullingResultData> m_SingleCullingResults = new();

    HashSet<BuildInRenderTextureData> m_BuildInRenderTextureDatas = new();
    List<CanSetGlobalResourceData> m_GlobalResources = new();
    List<TextureListData> m_TextureList = new();
    void PrepareResources() {
        using var profilingScope = new ProfilingScope(ProfilingSampler.Get(RPGProfileId.PrepareResources));
        if (needCrateSharedResource) {
            m_ResourceCreateEveryFrame.Clear();
            m_ResourceImportEveryFrame.Clear();
            m_RendererListDatas.Clear();
            m_BuildInRenderTextureDatas.Clear();
            m_GlobalResources.Clear();
            m_TextureList.Clear();
            cameraData.historyRTHandleSystem.ReleaseAll();
            foreach (ResourceData resourceData in asset.Graph.ResourceList) {
                bool used = false;
                foreach (NodeData nodeData in asset.Graph.NodeList) {
                    if (nodeData is ResourceNodeData resourceNodeData) {
                        if (resourceNodeData.Resource == resourceData && resourceNodeData.AttachTo.LinkTo.Count > 0) {
                            used = true;
                            break;
                        }
                    }
                    else if (nodeData is PassNodeData passNodeData) {
                        foreach (RPGParameterData parameterData in passNodeData.Parameters.Values) {
                            if (parameterData.UseDefault && parameterData.DefaultResource == resourceData) {
                                used = true;
                                break;
                            }
                        }
                    }
                }
                if (!used) continue;
                if (resourceData is CanSetGlobalResourceData canSetGlobalResourceData && canSetGlobalResourceData.ShaderPropertyIdStr != string.Empty) {
                    if (resourceData.usage is Usage.Imported or Usage.Shared) {
                        m_GlobalResources.Add(canSetGlobalResourceData);
                    }
                }
                switch (resourceData.usage) {
                    case Usage.Imported:
                        if (resourceData is BuildInRenderTextureData buildInRenderTextureData) {
                            m_BuildInRenderTextureDatas.Add(buildInRenderTextureData);
                        }
                        else if (resourceData is TextureListData textureListData) {
                            Func<RTHandleSystem, int, RTHandle> allocator = (RTHandleSystem rtHandleSystem, int i) => {
                                var rpgTextureDesc = textureListData.m_desc.value;
                                var desc = rpgTextureDesc.GetDescStruct();
                                desc.name += $"_{i}";
                                return rpgTextureDesc.sizeMode switch {
                                    TextureSizeMode.Explicit => rtHandleSystem.Alloc(desc.width, desc.height, desc.slices, desc.depthBufferBits, desc.colorFormat,
                                        desc.filterMode, desc.wrapMode, desc.dimension, desc.enableRandomWrite, desc.useMipMap, desc.autoGenerateMips, desc.isShadowMap,
                                        desc.anisoLevel, desc.mipMapBias, desc.msaaSamples, desc.bindTextureMS, desc.useDynamicScale, desc.useDynamicScaleExplicit,
                                        desc.memoryless, desc.vrUsage, desc.name),
                                    TextureSizeMode.Scale => rtHandleSystem.Alloc(desc.scale, desc.slices, desc.depthBufferBits, desc.colorFormat, desc.filterMode,
                                        desc.wrapMode, desc.dimension, desc.enableRandomWrite, desc.useMipMap, desc.autoGenerateMips, desc.isShadowMap, desc.anisoLevel,
                                        desc.mipMapBias, desc.msaaSamples, desc.bindTextureMS, desc.useDynamicScale, desc.useDynamicScaleExplicit, desc.memoryless,
                                        desc.vrUsage, desc.name),
                                    TextureSizeMode.Functor => rtHandleSystem.Alloc(desc.func, desc.slices, desc.depthBufferBits, desc.colorFormat, desc.filterMode,
                                        desc.wrapMode,
                                        desc.dimension, desc.enableRandomWrite, desc.useMipMap, desc.autoGenerateMips, desc.isShadowMap, desc.anisoLevel, desc.mipMapBias,
                                        desc.msaaSamples, desc.bindTextureMS, desc.useDynamicScale, desc.useDynamicScaleExplicit, desc.memoryless, desc.vrUsage,
                                        desc.name),
                                    _ => throw new ArgumentOutOfRangeException()
                                };

                            };

                            cameraData.historyRTHandleSystem.AllocBuffer(m_TextureList.Count, allocator, textureListData.bufferCount);
                            m_TextureList.Add(textureListData);
                        }
                        else {
                            m_ResourceImportEveryFrame.Add(resourceData);
                        }

                        break;
                    case Usage.Created:
                        m_ResourceCreateEveryFrame.Add(resourceData);
                        if (resourceData.type == ResourceType.RendererList && resourceData is RendererListData rendererListData) {
                            // to cull
                            m_RendererListDatas.Add(rendererListData);
                        }
                        else if (resourceData.type == ResourceType.CullingResult && resourceData is CullingResultData cullingResultData) {
                            m_SingleCullingResults.Add(cullingResultData);
                        }
                        break;
                    case Usage.Shared when resourceData is TextureData textureData:
                        textureData.handle = renderGraph.CreateSharedTexture(textureData.m_desc.value.GetDescStruct());
                        break;
                    default:
                        throw new Exception(
                            $"{Enum.GetName(typeof(Usage), resourceData.usage)} {Enum.GetName(typeof(ResourceType), resourceData.type)} is not supported.");
                }

            }
        }
    }

    protected virtual void UpdateVolumeFramework() {
        using var profilingScope = new ProfilingScope(ProfilingSampler.Get(RPGProfileId.UpdateVolumeFramework));
        // VolumeManager.instance.ResetMainStack();
        // "Default" LayerMask
        VolumeManager.instance.Update(camera.transform, layerMask: 1);
    }


    void Cull() {
        using var profilingScope = new ProfilingScope(ProfilingSampler.Get(RPGProfileId.CullingCPU));
        // testProfilingSampler
        if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview) {
            ScriptableRenderContext.EmitGeometryForCamera(camera);
        }
#if UNITY_EDITOR
        // emit scene view UI
        else if (camera.cameraType == CameraType.SceneView) {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif
        m_CullingResultsMap.Clear();
        foreach (RendererListData rendererListData in m_RendererListDatas) {
            RPGCullingDesc cullingDesc = rendererListData.m_CullingDesc;
            if (rendererListData.cullingFunc == null) {
                Type type = Type.GetType(cullingDesc.cullingFunctionTypeName);
                if (type == null) {
                    throw new InvalidOperationException();
                }
                rendererListData.cullingFunc = (CullingDefault)Activator.CreateInstance(type);
            }
            if (!m_CullingResultsMap.TryGetValue(cullingDesc, out var cullingResults)) {
                cullingResults = rendererListData.cullingFunc.Cull(camera, context);
                m_CullingResultsMap[cullingDesc] = cullingResults;
            }
            rendererListData.cullingResults = cullingResults;
        }
        foreach (CullingResultData singleCullingResult in m_SingleCullingResults) {
            RPGCullingDesc cullingDesc = singleCullingResult.m_CullingDesc;
            if (singleCullingResult.cullingFunc == null) {
                Type type = Type.GetType(cullingDesc.cullingFunctionTypeName);
                if (type == null) {
                    throw new InvalidOperationException();
                }
                singleCullingResult.cullingFunc = (CullingDefault)Activator.CreateInstance(type);
            }
            if (!m_CullingResultsMap.TryGetValue(cullingDesc, out var cullingResults)) {
                cullingResults = singleCullingResult.cullingFunc.Cull(camera, context);
                m_CullingResultsMap[cullingDesc] = cullingResults;
            }
            singleCullingResult.cullingResults = cullingResults;
        }
    }

    void ImportBuildInRenderTexture() {
        int numSamples = Mathf.Max(Screen.msaaSamples, 1);
        if (Application.isEditor)
            numSamples = 1;
        ImportResourceParams importBackbufferColorParams = new ImportResourceParams();
        importBackbufferColorParams.clearOnFirstUse = true;
        importBackbufferColorParams.clearColor = Color.clear;
        importBackbufferColorParams.discardOnLastUse = false;
        RenderTargetInfo importInfo = new RenderTargetInfo();
        importInfo.width = Screen.width;
        importInfo.height = Screen.height;
        importInfo.volumeDepth = 1;
        importInfo.msaaSamples = numSamples;
        // importInfo.format = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
        importInfo.format = GraphicsFormat.B10G11R11_UFloatPack32;

        foreach (BuildInRenderTextureData buildInRenderTextureData in m_BuildInRenderTextureDatas) {
            switch (buildInRenderTextureData.textureType) {
                case RPGBuildInRTType.CameraTarget:
                    var targetId = camera.targetTexture != null ? new RenderTargetIdentifier(camera.targetTexture) : BuiltinRenderTextureType.CameraTarget;
                    if (!buildInRenderTextureData.handle.IsValid()) {
                        // only need once?
                        buildInRenderTextureData.rtHandle ??= RTHandles.Alloc(targetId, "Backbuffer color");
                        RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref buildInRenderTextureData.rtHandle, targetId);
                        buildInRenderTextureData.handle = renderGraph.ImportTexture(buildInRenderTextureData.rtHandle, importInfo, importBackbufferColorParams);
                        Assert.IsTrue(buildInRenderTextureData.handle.IsValid());
                    }
                    break;
                case RPGBuildInRTType.CameraDepth: // TODO CameraDepth
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    void SetupCreatedResource(RenderGraph renderGraph) {
        foreach (ResourceData resourceData in m_ResourceCreateEveryFrame) {
            switch (resourceData.type) {
                case ResourceType.Texture when resourceData is TextureData textureData:
                    textureData.handle = renderGraph.CreateTexture(textureData.m_desc.value.GetDescStruct());
                    break;
                case ResourceType.Buffer when resourceData is BufferData bufferData:
                    bufferData.handle = renderGraph.CreateBuffer(bufferData.desc);
                    break;
                case ResourceType.RendererList when resourceData is RendererListData rendererListData:
                    CreateRendererList(rendererListData);
                    break;
                case ResourceType.CullingResult when resourceData is CullingResultData cullingResultData:
                    break;
                default:
                    throw new Exception(
                        $"{Enum.GetName(typeof(Usage), resourceData.usage)} {Enum.GetName(typeof(ResourceType), resourceData.type)} is not supported yet.");
            }
        }
    }

    static Material errorMaterial;
    void CreateRendererList(RendererListData rendererListData) {

        var desc = rendererListData.m_RenderListDesc.value;
        // if (desc.shaderTagIdStrs.Count == 0) {
        //     rendererListData.rendererList = RendererList.nullRendererList;
        //     return;
        // }

        var sortingSettings = new SortingSettings(camera) {
            criteria = desc.sortingCriteria
        };
#if UNITY_EDITOR
        errorMaterial ??= new(Shader.Find("Hidden/Core/FallbackError"));
#endif
        var drawingSettings = new DrawingSettings(
            RenderGraphUtils.GetShaderTagId(desc.shaderTagIdStrs[0]), sortingSettings
        ) {
            // overrideMaterial = errorMaterial,
            enableDynamicBatching = desc.enableDynamicBatching,
            perObjectData = desc.perObjectData
        };
        if (desc.useOverrideShader) {
            drawingSettings.overrideShader = Shader.Find(desc.overrideShaderPath);
        }
        if (desc.shaderTagIdStrs.Count >= 2) {
            for (int i = 1; i < desc.shaderTagIdStrs.Count; i++) {
                drawingSettings.SetShaderPassName(i, RenderGraphUtils.GetShaderTagId(desc.shaderTagIdStrs[i]));
            }
        }
        FilteringSettings filteringSettings = new(desc.renderQueueRange, desc.layerMask);
        var rlps = new RendererListParams(rendererListData.cullingResults, drawingSettings, filteringSettings);
        rendererListData.rendererList = renderGraph.CreateRendererList(rlps);
    }

    void SetupImportedResource(RenderGraph renderGraph) {
        foreach (ResourceData resourceData in m_ResourceImportEveryFrame) {
            switch (resourceData.type) {
                case ResourceType.Texture when resourceData is TextureData textureData:
                    textureData.handle = renderGraph.ImportTexture(textureData.rtHandle);
                    break;
                case ResourceType.Buffer when resourceData is BufferData bufferData:
                    bufferData.handle = renderGraph.ImportBuffer(bufferData.graphicsBuffer);
                    break;
                case ResourceType.AccelerationStructure when resourceData is RTAData rtaData:
                    rtaData.handle = renderGraph.ImportRayTracingAccelerationStructure(rtaData.accelStruct);
                    break;
                default:
                    throw new Exception(
                        $"{Enum.GetName(typeof(Usage), resourceData.usage)} {Enum.GetName(typeof(ResourceType), resourceData.type)} is not supported.");
            }
        }
        for (int i = 0; i < m_TextureList.Count; i++) {
            var textureListData = m_TextureList[i];
            textureListData.rtHandles ??= new();
            textureListData.handles ??= new();
            textureListData.rtHandles.Clear();
            textureListData.handles.Clear();
            for (int j = 0; j < textureListData.bufferCount; j++) {
                RTHandle rtHandle = cameraData.historyRTHandleSystem.GetFrameRT(i, j);
                textureListData.rtHandles.Add(rtHandle);
                textureListData.handles.Add(renderGraph.ImportTexture(rtHandle));
            }
        }
    }

    void SetupGlobalTexture(CommandBuffer cmd) {
        foreach (CanSetGlobalResourceData globalResourceData in m_GlobalResources) {
            int shaderPropertyId = RenderGraphUtils.GetShaderPropertyId(globalResourceData.ShaderPropertyIdStr);
            switch (globalResourceData) {
                case BufferData bufferData:
                    cmd.SetGlobalBuffer(shaderPropertyId, bufferData.graphicsBuffer);
                    break;
                case RTAData rtaData:
                    cmd.SetGlobalRayTracingAccelerationStructure(shaderPropertyId, rtaData.accelStruct);
                    break;
                case TextureData textureData:
                    cmd.SetGlobalTexture(shaderPropertyId, textureData.handle);
                    break;
            }
        }
    }

    void Submit() {
        ExecuteBuffer();
        using (new ProfilingScope(new ProfilingSampler(camera.name + ":context.submit"))) {
            context.Submit();
        }
    }

    void ExecuteBuffer() {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void Dispose() {

    }
}
