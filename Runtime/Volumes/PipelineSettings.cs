using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph.Runtime.Volumes {
    [Serializable, VolumeComponentMenu("PipelineSetting")]
    [SupportedOnRenderPipeline(typeof(RPGRenderPipelineAsset))]
    public class PipelineSetting:VolumeComponent {
        [Tooltip("Use Scriptable Render Pipeline Batching")]
        public BoolParameter useSRPBatching = new BoolParameter(true);
        [Tooltip("Lights Use Linear Intensity")]
        public BoolParameter lightsUseLinearIntensity = new BoolParameter(true);
    }
}
