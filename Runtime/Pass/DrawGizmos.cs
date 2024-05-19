using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class DrawGizmos : RPGPass {
        public class PassData {
            [Fragment, Depth]
            public TextureHandle depthAttachment;

            [Fragment]
            public TextureHandle colorAttachment;

            internal RendererListHandle GizmosListHandle;
        }

        public DrawGizmos() : base() {
        }

        public override bool Valid(Camera camera) {
#if UNITY_EDITOR
            if (camera.cameraType != CameraType.SceneView)
                return false;
            return Handles.ShouldRenderGizmos() && camera.sceneViewFilterMode != Camera.SceneViewFilterMode.ShowFiltered;
#else
            return false;
#endif
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var pd = passData as PassData;
            pd.GizmosListHandle = renderGraph.CreateGizmoRendererList(cameraData.camera, GizmoSubset.PostImageEffects);
            builder.UseRendererList(pd.GizmosListHandle);
            builder.AllowPassCulling(false);
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            context.cmd.DrawRendererList(passData.GizmosListHandle);
        }
    }
}
