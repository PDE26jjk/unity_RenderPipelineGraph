using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable, VolumeComponentMenu("Post-processing/testVolume")]
[SupportedOnRenderPipeline(typeof(RPGRenderPipelineAsset))]
public class testVolume : VolumeComponent {

    [Tooltip(
        "Adjusts the overall exposure of the scene in EV100. This is applied after HDR effect and right before tonemapping so it won't affect previous effects in the chain.")]
    public FloatParameter postExposure = new FloatParameter(0f);

}
