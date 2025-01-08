using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable, VolumeComponentMenu("Post-processing/testVolume")]
[SupportedOnRenderPipeline(typeof(RPGRenderPipelineAsset))]
// [VolumeComponentMenuForRenderPipeline("Post-processing/testVolume", typeof(RPGRenderPipeline))]
public class testVolume : VolumeComponent {

    [Tooltip(
        "Adjusts the overall exposure of the scene in EV100. This is applied after HDR effect and right before tonemapping so it won't affect previous effects in the chain.")]
    [Indent(0)]
    public FloatParameter postExposure = new FloatParameter(0f);

    public VolumeParameter<ComputeShader> computeShader = new ();
    
    public VolumeParameter<RayTracingShader> rayTracingShader = new ();
}
