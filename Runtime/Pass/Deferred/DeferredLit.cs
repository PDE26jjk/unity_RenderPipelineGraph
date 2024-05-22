using System;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class DeferredLit : RPGPass {
        static readonly Shader _shader = Shader.Find("MySRP/DeferredLit");
        static Material _material;
        static readonly int GBuffer0 = Shader.PropertyToID("_UnityFBInput0");
        static readonly int GBuffer1 = Shader.PropertyToID("_UnityFBInput1");
        static readonly int GBuffer2 = Shader.PropertyToID("_UnityFBInput2");
        static readonly int DepthMap = Shader.PropertyToID("_UnityFBInput3");
        public class PassData {

            [Input(0)]
            public TextureHandle BaseColor;
            [Input(1)]
            public TextureHandle Mix;
            [Input(2)]
            public TextureHandle Normal;
            [Input(3)]
            public TextureHandle DepthMap;
            [Read]
            public TextureHandle shadowMap1; // If shadowmaps are not marked, they may be recycled before the pass is executed
            [Read]
            public TextureHandle shadowMap2;

            [Fragment]
            public TextureHandle colorAttachment;
            internal bool NativePass;
        }
        public DeferredLit() : base() {
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            ((PassData)passData).NativePass = renderGraph.nativeRenderPassesEnabled;
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            if (_material == null) {
                _material = new Material(_shader);
            }
            Vector2 viewportScale = Vector2.one;
            Vector4 scaleBias = new Vector4(viewportScale.x, viewportScale.y, 0, 0);
            if (!passData.NativePass) {
                _material.SetTexture(GBuffer0, passData.BaseColor);
                _material.SetTexture(GBuffer1, passData.Mix);
                _material.SetTexture(GBuffer2, passData.Normal);
                _material.SetTexture(DepthMap, passData.DepthMap);
            } 
            _material.EnableKeyword("_RECEIVE_SHADOWS");
            Blitter.BlitTexture(context.cmd, passData.shadowMap1, scaleBias, _material, 0);
        }
    }
}
