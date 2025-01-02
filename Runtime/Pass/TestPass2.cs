using RenderPipelineGraph.Attribute;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestPass2 : RPGPass {
        public class PassData {
            [Hidden]
            public TextureHandle texture;
            [Fragment]
            public TextureHandle write1;
        }
        public TestPass2() : base() {
            this.m_AllowPassCulling = false;
        }
        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            ((PassData)passData).texture = renderGraph.ImportTexture(RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<Texture>("Assets/RPG/input.png")));
            builder.UseTexture(((PassData)passData).texture); 
        }
        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            Blitter.BlitTexture(cmd, passData.texture, new Vector4(1, 1, 0, 0), 0, false);
        }
    }
}
