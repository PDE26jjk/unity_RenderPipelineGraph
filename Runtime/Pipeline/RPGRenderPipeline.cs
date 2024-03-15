using System.Collections.Generic;
using System.Linq;
using PDE;
using RenderPipelineGraph;
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

    public RPGRenderPipeline(List<SerializableTagGraphPair> graphPairs) {
        foreach (var pair in graphPairs) {
            graphs[pair.tag] = pair.graph;
        }
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        GraphicsSettings.lightsUseLinearIntensity = true;
        VolumeManager.instance.Initialize(VolumeProfileSetting.GetOrCreateDefaultVolumeProfile());
        DebugManager.instance.RefreshEditor();
        InitializeForEditor();
    }
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        Lightmapping.ResetDelegate();
        // renderGraph.Cleanup();
        VolumeManager.instance.Deinitialize();
        Blitter.Cleanup();
        foreach (CameraData cameraData in CameraDataMap.Values) {
            cameraData.Dispose();
        }
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        Render(context, new List<Camera>(cameras));
    }
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras) {
        SortCamaras(cameras);

        BeginContextRendering(context, cameras);

        // if (!Asset.Deserialized) Asset.Deserialize();
        //
        // if (Asset.NeedRecompile || renderGraph is null) {
        //     renderGraph?.Cleanup();
        //     renderGraph = new("Some Render Graph");
        // }

        // Iterate over all Cameras
        foreach (Camera camera in cameras) {
            if (!graphs.TryGetValue(camera.tag, out var graph)) {
                if (camera.CompareTag("Untagged")) graph = graphs.Values.First();
                else
                    continue;
            }
            // renderer.Render(renderGraph, context, camera, shadowSettings, postFXSettings);

            if (!CameraDataMap.TryGetValue(camera, out var cameraData)) {
                CameraDataMap[camera] = cameraData = new CameraData(camera);
            }
            if (graph.NeedRecompile) cameraData.needReloadGraph = true;
            if (!graph.Deserialized) graph.Deserialize();
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
