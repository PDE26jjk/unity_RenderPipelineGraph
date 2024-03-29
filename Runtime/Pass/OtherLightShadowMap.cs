﻿using RenderPipelineGraph.Attribute;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class OtherLightShadowMap : RPGPass {

        public OtherLightShadowMap():base() {
            this.m_AllowGlobalStateModification = true;
        }
        public class PassData {
            [Depth]
            public TextureHandle shadowMap;
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var data = passData as PassData;
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            LightingHelper.instance.RenderOtherShadowMap(cmd);
        }
    }
}
