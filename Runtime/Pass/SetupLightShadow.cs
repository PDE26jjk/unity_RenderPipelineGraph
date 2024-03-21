using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Volume;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class SetupLightShadow : RPGPass {
        ShadowMapHelper.ShadowSettings m_ShadowSettings;

        public SetupLightShadow():base() {
            m_AllowGlobalStateModification = true;
        }

        public class PassData {
            [Default]
            public CullingResults cullingResults;
        }

        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var data = passData as PassData;
            var shadowSettingsVolume = VolumeManager.instance.stack.GetComponent<HDShadowSettings>();
            m_ShadowSettings ??= new ShadowMapHelper.ShadowSettings();
            m_ShadowSettings.maxDistance = shadowSettingsVolume.maxShadowDistance.value;
            m_ShadowSettings.directional.cascadeCount = shadowSettingsVolume.cascadeShadowSplitCount.value;
            float[] cascadeShadowSplits = shadowSettingsVolume.cascadeShadowSplits;
            m_ShadowSettings.directional.cascadeRatio1 = cascadeShadowSplits[0];
            m_ShadowSettings.directional.cascadeRatio2 = cascadeShadowSplits[1];
            m_ShadowSettings.directional.cascadeRatio3 = cascadeShadowSplits[2];
            // TODO Add other shadow settings
            LightingHelper.instance.Setup(data.cullingResults,m_ShadowSettings,renderGraph);
            LightingHelper.instance.RecordRendererLists(builder);
        }

        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
            LightingHelper.instance.SetGlobalLightingConstant(cmd);
        }
    }
}
