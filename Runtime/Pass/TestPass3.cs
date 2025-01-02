using RenderPipelineGraph.Attribute;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestPass3 : RPGPass {
        public class PassData {
            [Write]
            public TextureHandle texture;
            [Hidden]
            public int width;
            [Hidden]
            public int height;
        }
        public TestPass3() : base(PassNodeType.Compute) {
            this.m_AllowPassCulling = false;
        }
        static ComputeShader testCs;
        static int kernelID;
        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            // testCs = Resources.Load<ComputeShader>();
            var testVolume = VolumeManager.instance.stack.GetComponent<testVolume>();
            testCs = testVolume.computeShader.value;
            kernelID = testCs.FindKernel("t1");
            var tex = ((PassData)passData).texture;

            TextureDesc textureDesc = tex.GetDescriptor(renderGraph);
            Vector2Int dimensions = textureDesc.CalculateFinalDimensions();
            ((PassData)passData).width = dimensions.x;
            ((PassData)passData).height = dimensions.y;
        }
        public static void Record(PassData passData, ComputeGraphContext context) {
            var cmd = context.cmd;
            var tex = passData.texture;
            cmd.SetComputeIntParam(testCs, "width", passData.width);
            cmd.SetComputeIntParam(testCs, "height", passData.height);
            cmd.SetComputeTextureParam(testCs, kernelID, "tex", tex);
            cmd.DispatchCompute(testCs,kernelID,passData.width,passData.height,1);
        }
    }
}
