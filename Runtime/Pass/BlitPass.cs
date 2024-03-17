using System;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class BlitPass : RPGPass {
        static readonly Shader _shader = Shader.Find("MySRP/FinalBlit");
        static Material _material;
        public class PassData {

            [Read]
            public TextureHandle src;

            [Fragment]
            public TextureHandle dst;

        }
        public BlitPass() {
            PassType = PassNodeType.Raster;
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            if (_material == null) {
                _material = new Material(_shader); 
            }
            Vector2 viewportScale = Vector2.one;
            Vector4 scaleBias = new Vector4(viewportScale.x, viewportScale.y, 0, 0);
            Blitter.BlitTexture(context.cmd, passData.src, scaleBias, _material, 0);
        }
    }
}
