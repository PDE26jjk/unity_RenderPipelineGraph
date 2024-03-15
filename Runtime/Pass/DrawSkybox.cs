﻿using RenderPipelineGraph.Attribute;
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

            internal RendererListHandle SkyboxListHandle;
        }

        public DrawSkybox() {
            PassType = PassNodeType.Raster;
        }

        public override bool Valid(Camera camera) {
            return camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null;
        }

        public override void Setup(object passData, Camera camera, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var pd = passData as PassData;
            pd.SkyboxListHandle = renderGraph.CreateSkyboxRendererList(camera); // ??? where is it? // fixed in 2023.3.0b10 by unity
            builder.UseRendererList(pd.SkyboxListHandle);
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            cmd.DrawRendererList(passData.SkyboxListHandle);
        }
    }
}
