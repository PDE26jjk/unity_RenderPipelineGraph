using RenderPipelineGraph.Attribute;
using UnityEngine;
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

        public DrawGizmos() {
            PassType = PassNodeType.Raster;
        }

        public override bool Valid(Camera camera) {
            return camera.cameraType == CameraType.SceneView;
        }

        public override void Setup(object passData, Camera camera, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var pd = passData as PassData;
            pd.GizmosListHandle = renderGraph.CreateGizmoRendererList(camera, GizmoSubset.PostImageEffects);
            builder.UseRendererList(pd.GizmosListHandle);
            builder.AllowPassCulling(false);
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            context.cmd.DrawRendererList(passData.GizmosListHandle);
        }
    }
}
