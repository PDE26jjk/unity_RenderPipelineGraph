﻿using System;
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

            public bool yFlip;
        }
        string m_Name;
        public TestFinalBlitPass() {
            PassType = PassNodeType.Raster;
        }
        
        public override void LoadData(object passData,Camera camera) {
            var yflip = false;
            var cameraType = camera.cameraType;
            if (cameraType == CameraType.SceneView || cameraType == CameraType.Preview)
                yflip = true;
            ((PassData)passData).yFlip = yflip;
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            Vector2 viewportScale = Vector2.one;
            Vector4 scaleBias = !passData.yFlip ? new Vector4(viewportScale.x, -viewportScale.y, 0, viewportScale.y) : new Vector4(viewportScale.x, viewportScale.y, 0, 0);
            Blitter.BlitTexture(context.cmd, passData.colorAttachment, scaleBias, _material, 0);
        }
    }
}
