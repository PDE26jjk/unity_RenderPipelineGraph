using System;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class TestFinalBlitPass : RPGPass {
        static readonly Shader _shader = Shader.Find("MySRP/FinalBlit");
        static Material _material;
        public class PassData {

            [Read]
            public TextureHandle colorAttachment;

            [Fragment]
            public TextureHandle targetAttachment;

            internal bool yFlip;
        }
        public TestFinalBlitPass():base() {
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var yflip = false;
            var cameraType = cameraData.camera.cameraType;
            if (cameraType == CameraType.SceneView || cameraType == CameraType.Preview)
                yflip = true;
            ((PassData)passData).yFlip = yflip;
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            if (_material == null) {
                _material = new Material(_shader); 
            }
            Vector2 viewportScale = Vector2.one;
            Vector4 scaleBias = !passData.yFlip ?
                new Vector4(viewportScale.x, -viewportScale.y, 0, viewportScale.y) :
                new Vector4(viewportScale.x, viewportScale.y, 0, 0);
            Blitter.BlitTexture(context.cmd, passData.colorAttachment, scaleBias, _material, 0);
        }
    }
}
