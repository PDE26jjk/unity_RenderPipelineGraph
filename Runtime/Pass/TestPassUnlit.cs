using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestPassUnlit : RPGPass {
        public class PassData {

            [Fragment, Depth]
            public TextureHandle depthAttachment;

            [Fragment]
            public TextureHandle colorAttachment;

            [Default]
            public RendererList rendererList;
        }

        public override bool Valid() {
            return true;
        }
        
        public TestPassUnlit() {
            PassType = PassNodeType.Raster;
        }
        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            cmd.DrawRendererList(passData.rendererList);
        }
    }
}
