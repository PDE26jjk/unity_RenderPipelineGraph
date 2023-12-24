using RenderPipelineGraph.Attribute;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class SetupLightShadow : RPGPass {

        public SetupLightShadow() {
            m_AllowGlobalStateModification = true;
        }

        public class PassData {
            [Default]
            public CullingResults cullingResults;
        }

        public override void Setup(object passData, Camera camera, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var data = passData as PassData;
            var shadowSettings = new ShadowMapHelper.ShadowSettings();
            shadowSettings.maxDistance = 20;
            LightingHelper.instance.Setup(data.cullingResults,shadowSettings,renderGraph);
            LightingHelper.instance.RecordRendererLists(builder);
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            LightingHelper.instance.SetGlobalLightingConstant(cmd);
        }
    }
}
