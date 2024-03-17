using System;
using System.Collections.Generic;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class TemporalAATest : RPGPass {
        static readonly Shader _shader = Shader.Find("MySRP/TaaTest");
        static Material _material;
        static readonly int LastFrame = Shader.PropertyToID("lastFrame");
        public class PassData {

            [Read]
            public TextureHandle colorAttachment;

            [Read]
            public TextureHandle motionVector;
            
            [Read]
            public TextureHandle lastFrame;
            
            [Fragment]
            public TextureHandle testTarget;
        }
        public TemporalAATest() {
            PassType = PassNodeType.Raster;
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {

        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            if (_material == null) {
                _material = new Material(_shader);
            }
            _material.SetTexture(LastFrame, passData.lastFrame);
            _material.SetTexture("_MotionVectorMap", passData.motionVector);
            Vector4 scaleBias = new Vector4(1 , 1, 0, 0);
            Blitter.BlitTexture(context.cmd, passData.colorAttachment, scaleBias, _material, 0);
        }
    }
}
