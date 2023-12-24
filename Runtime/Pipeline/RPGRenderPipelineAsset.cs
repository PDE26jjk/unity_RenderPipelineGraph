using System;
using RenderPipelineGraph;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/myRenderPipelineAsset")]
public class RPGRenderPipelineAsset : RenderPipelineAsset<RPGRenderPipeline> {

    [SerializeField]
    ShadowSettingsLegacy shadows = default;
    [SerializeField]
    PostFXSettings postFXSettings = default;

    [SerializeField]
    RPGAsset RpgAsset = default;

    protected override UnityEngine.Rendering.RenderPipeline CreatePipeline() {
        Debug.Log("createPipeline");
        RpgAsset.Graph.TestInit3();
        try {
            Blitter.Initialize(Shader.Find("MySRP/FinalBlit"), Shader.Find("MySRP/FinalBlit"));
        }
        catch {
        }
        return new RPGRenderPipeline(ref RpgAsset,shadows, postFXSettings);

    }
    protected void OnDestroy() {
    }
}
