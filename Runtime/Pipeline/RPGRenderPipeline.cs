using System.Collections.Generic;
using System.Linq;
using PDE;
using RenderPipelineGraph;
using RenderPipelineGraph.Runtime.Volumes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.RenderGraphModule;
using LightType = UnityEngine.LightType;
public partial class RPGRenderPipeline : UnityEngine.Rendering.RenderPipeline {

    Dictionary<string, RPGAsset> graphs = new();

    // RenderGraph renderGraph = new RenderGraph("RPG");
    VolumeProfile defaultProfile;
    Dictionary<Camera, CameraData> CameraDataMap = new();
    RPGRenderPipelineAsset m_PipelineAsset;
    public RPGRenderPipeline( RPGRenderPipelineAsset pipelineAsset) {
        m_PipelineAsset = pipelineAsset;
        foreach (var pair in m_PipelineAsset.cameraRenderGraphs) {
            graphs[pair.tag] = pair.graph;
        }
        VolumeManager.instance.Initialize(VolumeProfileSetting.GetOrCreateDefaultVolumeProfile());
        DebugManager.instance.RefreshEditor();
        InitializeForEditor();
        // RTHandles.Initialize(1,1);
    }
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        foreach (CameraData cameraData in CameraDataMap.Values) {
            cameraData.Dispose();
        }
        _ScreenCameraData?.Dispose();
        _PreviewCameraData?.Dispose();
        Lightmapping.ResetDelegate();
        // renderGraph.Cleanup();
        VolumeManager.instance.Deinitialize();
        Blitter.Cleanup();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        Render(context, new List<Camera>(cameras));
    }
    CameraData _ScreenCameraData;
    CameraData _PreviewCameraData;
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras) {
        SortCamaras(cameras);

        BeginContextRendering(context, cameras);

        // if (!Asset.Deserialized) Asset.Deserialize();
        //
        // if (Asset.NeedRecompile || renderGraph is null) {
        //     renderGraph?.Cleanup();
        //     renderGraph = new("Some Render Graph");
        // }
        var settings = VolumeManager.instance.stack.GetComponent<PipelineSetting>();
        GraphicsSettings.useScriptableRenderPipelineBatching = settings.useSRPBatching.value;
        GraphicsSettings.lightsUseLinearIntensity = settings.lightsUseLinearIntensity.value;
        
        // Iterate over all Cameras
        foreach (Camera camera in cameras) {
            if (!graphs.TryGetValue(camera.tag, out var graph)) {
                if (camera.CompareTag("Untagged")) graph = graphs.Values.First();
                else
                    continue;
            }
            // renderer.Render(renderGraph, context, camera, shadowSettings, postFXSettings);
            CameraData cameraData;
            if (camera.cameraType == CameraType.SceneView) {
                cameraData = _ScreenCameraData ??= new CameraData(camera);
                _ScreenCameraData.m_Camera = camera;
            }else if (camera.cameraType == CameraType.Preview) {
                cameraData = _PreviewCameraData ??= new CameraData(camera);
                _PreviewCameraData.m_Camera = camera;
            }
            else if (!CameraDataMap.TryGetValue(camera, out cameraData)) {
                CameraDataMap[camera] = cameraData = new CameraData(camera);
                // Debug.Log(camera.GetInstanceID()); 
            }
            if (graph.NeedRecompile) cameraData.needReloadGraph = true;
            if (!graph.Deserialized) graph.Deserialize();

            cameraData.renderGraph.nativeRenderPassesEnabled = m_PipelineAsset.useNRP;
            cameraData.Render(graph, context);
        }
        foreach (RPGAsset graph in graphs.Values) {
            foreach (RPGPass pass in graph.m_Graph.NodeList.OfType<PassNodeData>().Select(t => t.Pass)) {
                pass.EndFrame();
            }
            if (graph.NeedRecompile) {
                graph.NeedRecompile = false;
            }
        }

        // renderGraph.EndFrame();

        EndContextRendering(context, cameras);
        // context.Submit();
    }

    protected virtual void SortCamaras(List<Camera> cameras) {

    }
}
