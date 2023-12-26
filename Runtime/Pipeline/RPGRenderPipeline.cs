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

    RPGAsset Asset;

    readonly RenderGraph renderGraph = new("Some Render Graph");
    public RPGRenderPipeline(ref RPGAsset asset) {
        this.Asset = asset;
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        GraphicsSettings.lightsUseLinearIntensity = true;

        VolumeManager.instance.Initialize(VolumeProfileSetting.GetOrCreateDefaultVolumeProfile());
        DebugManager.instance.RefreshEditor();
        InitializeForEditor();
    }
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        Lightmapping.ResetDelegate();
        renderGraph.Cleanup();
        VolumeManager.instance.Deinitialize();
        Blitter.Cleanup();
        m_RpgRenderer.Dispose();
    }
    RPGRenderer m_RpgRenderer = new();

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        Render(context,new List<Camera>(cameras));
    }
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras) {
        SortCamaras(cameras);
        
        BeginContextRendering(context, cameras);
        
        // Iterate over all Cameras
        foreach (Camera camera in cameras) {
            // renderer.Render(renderGraph, context, camera, shadowSettings, postFXSettings);
            m_RpgRenderer.Render(Asset,renderGraph,context,camera);
        }
        
        foreach (RPGPass pass in Asset.m_Graph.NodeList.OfType<PassNodeData>().Select(t=>t.Pass)) {
            pass.EndFrame();
        }
        
        renderGraph.EndFrame();
        
        EndContextRendering(context, cameras);
        // context.Submit();
    }

    protected virtual void SortCamaras(List<Camera> cameras) {
        
    }
}
