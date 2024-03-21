using System;
using System.Collections.Generic;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class TemporalAAUpdate : RPGPass {
        static readonly Shader _shader = Shader.Find("MySRP/TaaUpdate");
        static Material _material;
        static readonly int LastFrame = Shader.PropertyToID("lastFrame");
        public class PassData {

            [Read]
            public TextureHandle colorAttachment;

            [Read]
            public TextureHandle depthAttachment;
            
            [Read]
            public TextureHandle motionVector;

            [ListSize(2), Read(new[] {
                1
            }), Fragment(0, 0)]
            public List<TextureHandle> TAABuffers; 
        }
        public TemporalAAUpdate():base() {
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {

        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            if (_material == null) {
                _material = new Material(_shader);
            }
            _material.SetTexture(LastFrame, passData.TAABuffers[1]);
            _material.SetTexture("_MotionVectorMap", passData.motionVector);
            Vector4 scaleBias = new Vector4(1 , 1, 0, 0);
            Blitter.BlitTexture(context.cmd, passData.colorAttachment, scaleBias, _material, 0);
        }
    }
}
