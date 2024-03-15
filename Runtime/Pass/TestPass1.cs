using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestPass1 : RPGPass {
        public class PassData {
            [Read]
            public TextureHandle read1;
            [Write, Fragment]
            public TextureHandle write1;
        }

        public TestPass1() {
            PassType = PassNodeType.Raster;
        }
        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            Blitter.BlitTexture(cmd, passData.read1, new Vector4(1, 1, 0, 0), 0, false);
        }
    }
}
