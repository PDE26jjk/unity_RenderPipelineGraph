using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestPassUnlit : RPGPass {
        public class PassData {

            [Fragment]
            public TextureHandle colorAttachment;
            [Fragment, Depth]
            public TextureHandle depthAttachment;

            [Read]
            public TextureHandle shadowMap1;// If shadowmaps are not marked, they may be recycled before the pass is executed
            [Read]
            public TextureHandle shadowMap2;


            [Default]
            public RendererListHandle rendererList;
        }

        public TestPassUnlit():base() {
        }
        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            cmd.DrawRendererList(passData.rendererList);
        }
    }
}
