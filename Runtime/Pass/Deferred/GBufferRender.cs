using RenderPipelineGraph;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace DeferredRender {
    public class GBufferRender : RPGPass {
        public class PassData {

            [Fragment(0)]
            public TextureHandle BaseColor;
            [Fragment(1)]
            public TextureHandle Mix;
            [Fragment(2)]
            public TextureHandle Normal;
            [Fragment(3)]
            public TextureHandle ColorAttachment;
            [Fragment(4)]
            public TextureHandle DepthMap;
            [Depth]
            public TextureHandle DepthAttachment;

            [Default]
            public RendererListHandle rendererList;
        }
        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            
        }

        public GBufferRender():base() {
            // PassType = PassNodeType.Raster;
        }
        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            cmd.DrawRendererList(passData.rendererList);
        }
    }
}
