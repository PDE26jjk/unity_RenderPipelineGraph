using RenderPipelineGraph.Volume;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class CullingDefault {
        public virtual CullingResults Cull(Camera camera,ScriptableRenderContext context) {
            camera.TryGetCullingParameters(out var cullingParameters);
            var shadowSettings = VolumeManager.instance.stack.GetComponent<HDShadowSettings>();
            
            cullingParameters.shadowDistance = Mathf.Min(shadowSettings.maxShadowDistance.value, camera.farClipPlane);
            return context.Cull(ref cullingParameters);
        }
    }
}
