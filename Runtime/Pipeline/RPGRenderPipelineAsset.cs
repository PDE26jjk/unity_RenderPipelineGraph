using System;
using System.Collections.Generic;
using RenderPipelineGraph;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[Serializable]
public class SerializableTagGraphPair {
    public string tag;
    public RPGAsset graph;
}
[CreateAssetMenu(menuName = "Rendering/myRenderPipelineAsset")]
public class RPGRenderPipelineAsset : RenderPipelineAsset<RPGRenderPipeline> {
    [SerializeField]
    RPGAsset RpgAsset = default;
    public VolumeProfile defaultVolumeProfile;


    // tag->graph
    [SerializeField]
    List<SerializableTagGraphPair> cameraRenderGraphs = default;

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
        return new RPGRenderPipeline(cameraRenderGraphs);
    }
    protected void OnDestroy() {
    }
}
