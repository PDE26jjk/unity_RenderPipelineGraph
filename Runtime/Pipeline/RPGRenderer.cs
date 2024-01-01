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

    internal Camera camera;

    internal CommandBuffer cmd;

    BufferedRTHandleSystem bufferedRTHandleSystem;

    // TextureHandle sharedTex;

    bool needCrateSharedResource =>asset.NeedRecompile;
    HashSet<ResourceData> m_ResourceCreateEveryFrame = new();
    HashSet<ResourceData> m_ResourceImportEveryFrame = new();
    bool needReorderPass =>asset.NeedRecompile;

    static Comparer<PassSortData> PassNodeComparer = Comparer<PassSortData>.Create((x, y) => {
        int compareTo = x.pos.x.CompareTo(y.pos.x);
        if (compareTo == 0) {
            compareTo = x.pos.y.CompareTo(y.pos.y);
        }
        if (compareTo == 0) return 1;
        return compareTo;
    });
    internal class PassSortData {
        public Vector2 pos;
        public SortedSet<PassSortData> followings = new(PassNodeComparer);
        public PassNodeData passNodeData;
        public bool read = false;
        public MethodInfo addRenderPassMethodInfo;
        public int refCount;
    }
    List<PassSortData> passSorted = new();
    RPGAsset asset;
    RenderGraph renderGraph;

    Dictionary<Camera, CameraData> CameraDataMap = new();
    class CameraData : IDisposable {
        internal readonly RTHandleSystem rtHandleSystem = new();
        internal readonly BufferedRTHandleSystem historyRTHandleSystem = new();
        internal Camera m_Camera;
        public void Dispose() {
            RestoreRTHandels();
            rtHandleSystem.Dispose();
            historyRTHandleSystem.Dispose();
        }
        internal Vector2Int sizeInPixel = Vector2Int.zero;
        FieldInfo m_DefaultRTHandlesInstanceInfo;
        RTHandleSystem m_DefaultRTHandles;
        public CameraData(Camera cam) {
            this.m_Camera = cam;
        }

        public void BorrowRTHandles() {
            m_DefaultRTHandlesInstanceInfo ??= typeof(RTHandles).GetField("s_DefaultInstance", BindingFlags.Static | BindingFlags.NonPublic);

            m_DefaultRTHandles ??= m_DefaultRTHandlesInstanceInfo.GetValue(null) as RTHandleSystem;

            m_DefaultRTHandlesInstanceInfo?.SetValue(null, this.rtHandleSystem);
        }
        public void RestoreRTHandels() {
            m_DefaultRTHandlesInstanceInfo?.SetValue(null, m_DefaultRTHandles);
        }

        public void SetReferenceSize() {
            if (sizeInPixel.x != m_Camera.pixelWidth
                || sizeInPixel.y != m_Camera.pixelHeight) {
                sizeInPixel.x = m_Camera.pixelWidth;
                sizeInPixel.y = m_Camera.pixelHeight;
                RTHandles.ResetReferenceSize(sizeInPixel.x, sizeInPixel.y);
                historyRTHandleSystem.ResetReferenceSize(sizeInPixel.x, sizeInPixel.y);
            }
        }
    }

    public void Render(RPGAsset asset, RenderGraph renderGraph, ScriptableRenderContext context, Camera camera) {
        this.context = context;
        this.camera = camera;
        this.asset = asset;
        this.renderGraph = renderGraph;
        cmd = CommandBufferPool.Get();

        if (!CameraDataMap.TryGetValue(camera, out var cameraData)) {
            CameraDataMap[camera] = cameraData = new CameraData(camera);
        }
        cameraData.BorrowRTHandles();
        cameraData.SetReferenceSize();
        // RTHandles.SetReferenceSize(camera.pixelWidth, camera.pixelHeight);

        var renderGraphParameters = new RenderGraphParameters {
            commandBuffer = cmd,
            currentFrameIndex = Time.frameCount,
            executionName = "Render RPG",
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

        ReorderPasses();

        // renderGraph.nativeRenderPassesEnabled = true;
        renderGraph.BeginRecording(renderGraphParameters);

        UpdateVolumeFramework();

        Cull();

        ImportBuildInRenderTexture();

        SetupCreatedResource(renderGraph);

        SetupImportedResource(renderGraph);

        SetupGlobalTexture(cmd);


        foreach (PassSortData passSortData in passSorted) {
            var passNodeData = passSortData.passNodeData;
            var pass = passNodeData.m_Pass;
            if (!pass.Valid()) continue;

            object[] parameters = RenderGraphUtils.MakeAddRenderPassParm(renderGraph, pass);
            using (var baseBuilder = passSortData.addRenderPassMethodInfo.Invoke(renderGraph, parameters) as IBaseRenderGraphBuilder) {
                baseBuilder.AllowPassCulling(pass.AllowPassCulling);
                baseBuilder.AllowGlobalStateModification(pass.AllowGlobalStateModification);

                object passData = parameters[1];
                switch (pass.PassType) {
                    case PassNodeType.Legacy:
                        break;
                    case PassNodeType.Unsafe:
                        break;
                    case PassNodeType.Raster:
                    {
                        var builder = baseBuilder as IRasterRenderGraphBuilder;
                        RenderGraphUtils.SetRenderFunc(builder, pass);
                        RenderGraphUtils.LoadPassData(passNodeData, passData, builder, renderGraph, this.camera);
                    }
                        break;
                    case PassNodeType.Compute:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        BaseRenderFunc<Object, RasterGraphContext> voidRenderFunc = (Object _, RasterGraphContext __) => { };

        TextureHandle defaultTex = renderGraph.CreateTexture(testDesc);
        TextureHandle defaultDepth = renderGraph.CreateTexture(testDepthDesc);
        // using (var builder = renderGraph.AddRasterRenderPass("r1", out Object _)) {
        //     builder.AllowPassCulling(false);
        //     // builder.UseTexture(defaultTex,AccessFlags.Write);
        //     builder.SetInputAttachment(defaultTex, 0);
        //     builder.SetRandomAccessAttachment(sharedTex, 1);
        //     builder.SetRenderAttachmentDepth(defaultDepth);
        //     builder.SetRenderFunc(voidRenderFunc);
        // }
        using (var builder = renderGraph.AddRasterRenderPass("r2", out Object _)) {
            // builder.AllowPassCulling(false);
            builder.SetRenderAttachment(defaultTex, 0);
            // builder.SetRenderAttachmentDepth(defaultDepth);
            builder.SetRenderFunc(voidRenderFunc);
        }

        renderGraph.EndRecordingAndExecute();
        renderGraph.nativeRenderPassesEnabled = false;

        // Cleanup();
        Submit();
        CommandBufferPool.Release(renderGraphParameters.commandBuffer);
        cameraData.RestoreRTHandels();
    }

    void ReorderPasses() {

        if (needReorderPass) {
            // needReorderPass = false;
            List<PassSortData> l1 = new();
            passSorted.Clear();
            Dictionary<PassNodeData, PassSortData> node2sort = new();

            foreach (NodeData nodeData in asset.Graph.NodeList) {
                if (nodeData is PassNodeData passNodeData) {
                    PassSortData passSortData = new() {
                        pos = passNodeData.pos,
                        passNodeData = passNodeData
                    };
                    l1.Add(passSortData);
                    node2sort[passNodeData] = passSortData;
                }
            }

            PassSortData root = new();
            // Add dependence info.
            foreach (PassSortData passSortData in l1) {
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
#if UNITY_EDITOR
            var str = new StringBuilder("Pass order: ");
            str.AppendJoin(", ", passSorted.Select(t => t.passNodeData.exposedName));
            Debug.Log(str.ToString());
#endif
            foreach (PassSortData passSortData in passSorted) {
                passSortData.addRenderPassMethodInfo = RenderGraphUtils.GetAddRasterRenderPassMethodInfo(renderGraph, passSortData.passNodeData.m_Pass);

                // addRenderPassMethodInfo?.Invoke(renderGraph,new object[]{});
            }
        }
    }
    Dictionary<RPGCullingDesc, CullingResults> m_CullingResultsMap = new();
    List<RendererListData> m_RendererListDatas = new();
    List<CullingResultData> m_SingleCullingResults = new();

    HashSet<BuildInRenderTextureData> m_BuildInRenderTextureDatas = new();
    List<CanSetGlobalResourceData> m_GlobalResources = new();
    void PrepareResources() {

        if (needCrateSharedResource) {
            // hasCrateSharedResource = true;
            m_ResourceCreateEveryFrame.Clear();
            m_ResourceImportEveryFrame.Clear();
            m_RendererListDatas.Clear();
            m_BuildInRenderTextureDatas.Clear();
            m_GlobalResources.Clear();
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
        // VolumeManager.instance.ResetMainStack();
        // "Default" LayerMask
        VolumeManager.instance.Update(camera.transform, layerMask: 1);
    }

    void Cull() {
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
                case RPGBuildInRTType.CameraDepth:
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
                        $"{Enum.GetName(typeof(Usage), resourceData.usage)} {Enum.GetName(typeof(ResourceType), resourceData.type)} is not supported.");
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

        errorMaterial ??= new(Shader.Find("Hidden/Core/FallbackError"));

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
        using (new ProfilingScope(new ProfilingSampler(camera.name + "context.submit"))) {
            context.Submit();
        }
    }

    void ExecuteBuffer() {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void Dispose() {
        foreach (var keyValuePair in CameraDataMap) {
            CameraData cameraData = keyValuePair.Value;
            cameraData.Dispose();
        }
    }
}
