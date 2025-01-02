using System;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class MotionVectorFromDepth : RPGPass {
        static readonly Shader _shader = Shader.Find("MySRP/MotionVectorFromDepth");
        static Material _material;
        public class PassData {

            [Read]
            public TextureHandle depthMap;

            [Fragment]
            public TextureHandle motionVectorMap;

        }
        public MotionVectorFromDepth():base() {
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var pd = passData as PassData;
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            if (_material == null) {
                _material = new Material(_shader); 
            }
            Vector4 scaleBias = new Vector4(1, 1, 0, 0);
            Blitter.BlitTexture(cmd, passData.depthMap, scaleBias, _material, 0);
        }
    }
}
