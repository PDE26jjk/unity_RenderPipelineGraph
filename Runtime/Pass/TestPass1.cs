using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestPass1:RPGPass {
        public class Inputs {
            public TextureHandle t1;
            public TextureHandle t2;
        }
        public class Outputs {
            public TextureHandle t3;
            public TextureHandle t4;
        }
        public TestPass1() {
            PassType = PassNodeType.Raster;
        }
        public string str;
        public static void Run(RasterGraphContext context,Inputs inputs,Outputs outputs) {
            var cmd = context.cmd;
            Blitter.BlitTexture(cmd,inputs.t1, new Vector4(1,1,0,0),0,false);
        }
    }
}
