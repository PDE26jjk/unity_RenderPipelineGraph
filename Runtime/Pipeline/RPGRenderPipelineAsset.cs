using System;
using RenderPipelineGraph;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/myRenderPipelineAsset")]
public class RPGRenderPipelineAsset : RenderPipelineAsset<RPGRenderPipeline> {
    [SerializeField]
    RPGAsset RpgAsset = default;
    public VolumeProfile defaultVolumeProfile;

    protected override UnityEngine.Rendering.RenderPipeline CreatePipeline() {
        Debug.Log("createPipeline");
        // RpgAsset.Graph.TestInit3();
        RpgAsset.NeedRecompile = true;
        RpgAsset.Deserialized = false;
        try {
            Blitter.Initialize(Shader.Find("MySRP/FinalBlit"), Shader.Find("MySRP/FinalBlit"));
        }
        catch {
        }
        return new RPGRenderPipeline(ref RpgAsset);
    }
    protected void OnDestroy() {
    }
}
