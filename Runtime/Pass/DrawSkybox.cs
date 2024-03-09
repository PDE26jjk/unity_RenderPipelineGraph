using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class DrawSkybox : RPGPass {
        public class PassData {
            [Fragment, Depth]
            public TextureHandle depthAttachment;

            [Fragment]
            public TextureHandle colorAttachment;

            internal bool skip;

            internal RendererListHandle SkyboxListHandle;
        }

        public override bool Valid() {
            return true;
        }

        public DrawSkybox() {
            PassType = PassNodeType.Raster;
        }
        
        public override void Setup(object passData, Camera camera, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var pd = passData as PassData;
            pd.skip = true;
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null) {
                // SkyboxListHandle = renderGraph.CreateSkyboxRendererList()// ??? where is it? // fix  in 2023.3.0b10 by unity
                // cmd.DrawRendererList(context.renderContext.CreateSkyboxRendererList(camera));
                pd.skip = false;
            }
        }
        
        public static void Record(PassData passData, RasterGraphContext context) {
            if(passData.skip) return;
            var cmd = context.cmd;
            cmd.DrawRendererList(passData.SkyboxListHandle);
        }
    }
}
