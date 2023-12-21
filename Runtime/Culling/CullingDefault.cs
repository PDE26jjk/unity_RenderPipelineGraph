using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class CullingDefault {
        public virtual CullingResults Cull(Camera camera,ScriptableRenderContext context) {
            camera.TryGetCullingParameters(out var cullingParameters);
            return context.Cull(ref cullingParameters);
        }
    }
}
