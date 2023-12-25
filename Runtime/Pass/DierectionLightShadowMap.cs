using RenderPipelineGraph.Attribute;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class DirectionLightShadowMap : RPGPass {

        public DirectionLightShadowMap() {
            this.PassType = PassNodeType.Raster;
            this.m_AllowGlobalStateModification = true;
        }
        public class PassData {
            [Fragment]
            public TextureHandle shadowMap;
        }

        public override void Setup(object passData, Camera camera, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var data = passData as PassData;
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            LightingHelper.instance.RenderDirShadowMap(cmd);
        }
    }
}
