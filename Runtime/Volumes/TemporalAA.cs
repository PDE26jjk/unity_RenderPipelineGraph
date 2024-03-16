using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph.Runtime.Volumes {
    [Serializable, VolumeComponentMenu("TemporalAA")]
    [SupportedOnRenderPipeline(typeof(RPGRenderPipelineAsset))]
    public class TemporalAA:VolumeComponent {
        [Tooltip("TAA switch")]
        public BoolParameter useTAA = new BoolParameter(true);
        [Tooltip("scale sample pixel size")]
        public FloatParameter sampleScale = new (1);
    }
}
