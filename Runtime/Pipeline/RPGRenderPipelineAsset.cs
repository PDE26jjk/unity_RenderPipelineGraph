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
    public VolumeProfile defaultVolumeProfile;
    [Tooltip("bug:turn it on need to restart editor")]
    public bool useNRP;
    // tag->graph
    [SerializeField]
    internal List<SerializableTagGraphPair> cameraRenderGraphs = default;

    protected override UnityEngine.Rendering.RenderPipeline CreatePipeline() {
        Debug.Log("createPipeline");
        // RpgAsset.Graph.TestInit3();
        foreach (var pair in cameraRenderGraphs) {
            pair.graph.NeedRecompile = true;
            pair.graph.Deserialized = false;
        }
        try {
            Blitter.Initialize(Shader.Find("MySRP/FinalBlit"), Shader.Find("MySRP/FinalBlit"));
        }
        catch {
        }
        return new RPGRenderPipeline(this);
    }
    protected void OnDestroy() {
    }
}
