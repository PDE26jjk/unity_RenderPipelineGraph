using System.Collections.Generic;
using RenderPipelineGraph.Attribute;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestPass4 : RPGPass {
        public class PassData {
            [Write]
            public TextureHandle outputTexture;
            internal Vector3 cameraPosition;
            internal Matrix4x4 pixelToWorldMatrix;
            internal RayTracingAccelerationStructure accelerationStructure;
            internal int texWidth;
            internal int texHeight;
        }
        public TestPass4() : base(PassNodeType.Compute) {
            this.m_AllowPassCulling = false;
        }
        static RayTracingShader testRs;
        static int kernelID;
        public override bool Valid(Camera camera) {
            if (camera.cameraType == CameraType.Preview)
                return false;
            bool supportsRayTracing = SystemInfo.supportsRayTracing;
            return supportsRayTracing;
        }

        Matrix4x4 GetPixelToWorldMatrix(Camera camera) {
            float aspectRatio = camera.aspect;
            float tanHalfVertFoV = Mathf.Tan(0.5f * camera.fieldOfView * Mathf.Deg2Rad);

            // Compose the matrix.
            float m21 = tanHalfVertFoV;
            float m11 = -2.0f / camera.pixelHeight * tanHalfVertFoV;

            float m20 = tanHalfVertFoV * aspectRatio;
            float m00 = -2.0f / camera.pixelWidth * tanHalfVertFoV * aspectRatio;


            Matrix4x4 viewSpaceRasterTransform = new Matrix4x4(new Vector4(m00, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, m11, 0.0f, 0.0f),
                new Vector4(m20, m21, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            var worldToViewMatrix = camera.worldToCameraMatrix;
            // Remove the translation component.
            var homogeneousZero = new Vector4(0, 0, 0, 1);
            worldToViewMatrix.SetColumn(3, homogeneousZero);

            // Flip the Z to make the coordinate system left-handed.
            worldToViewMatrix.SetRow(2, -worldToViewMatrix.GetRow(2));

            // Transpose for HLSL.
            return Matrix4x4.Transpose(worldToViewMatrix.transpose * viewSpaceRasterTransform);
        }
        RayTracingInstanceCullingTest GI_CT = new RayTracingInstanceCullingTest();
        RayTracingInstanceCullingConfig cullingConfig = new();
        public List<RayTracingInstanceCullingTest> instanceTestArray = new List<RayTracingInstanceCullingTest>();
        RayTracingAccelerationStructure rtas = null;
        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            var testVolume = VolumeManager.instance.stack.GetComponent<testVolume>();
            testRs = testVolume.rayTracingShader.value;
            var _passData = passData as PassData;
            _passData.cameraPosition = cameraData.m_Camera.transform.position;
            _passData.pixelToWorldMatrix = GetPixelToWorldMatrix(cameraData.m_Camera);
            var camera = cameraData.m_Camera;
            cullingConfig.flags = RayTracingInstanceCullingFlags.None;
            cullingConfig.sphereRadius = 1000.0f;
            cullingConfig.sphereCenter = _passData.cameraPosition;
            
            cullingConfig.lodParameters.fieldOfView = camera.fieldOfView;
            cullingConfig.lodParameters.cameraPosition = camera.transform.position;
            cullingConfig.lodParameters.cameraPixelHeight = camera.pixelHeight;
            cullingConfig.flags |= RayTracingInstanceCullingFlags.EnableLODCulling | RayTracingInstanceCullingFlags.IgnoreReflectionProbes;
            
            cullingConfig.subMeshFlagsConfig.opaqueMaterials = RayTracingSubMeshFlags.Enabled | RayTracingSubMeshFlags.ClosestHitOnly;
            cullingConfig.materialTest.requiredShaderTags = new RayTracingInstanceCullingShaderTagConfig[1];
            cullingConfig.materialTest.requiredShaderTags[0].tagId = new ShaderTagId("RenderPipeline");
            cullingConfig.materialTest.requiredShaderTags[0].tagValueId = new ShaderTagId("RPG");
            GI_CT.allowOpaqueMaterials = true;
            GI_CT.allowAlphaTestedMaterials = true;
            GI_CT.allowTransparentMaterials = true;
            GI_CT.layerMask = -1;
            GI_CT.shadowCastingModeMask = (1 << (int)ShadowCastingMode.Off) | (1 << (int)ShadowCastingMode.On) | (1 << (int)ShadowCastingMode.TwoSided);
            GI_CT.instanceMask = 1;
            GI_CT.allowVisualEffects = true;
            instanceTestArray.Clear();
            instanceTestArray.Add(GI_CT);
            if (cullingConfig.instanceTests == null ||cullingConfig.instanceTests.Length != instanceTestArray.Count)
                cullingConfig.instanceTests = instanceTestArray.ToArray();
            else
                instanceTestArray.CopyTo(0, cullingConfig.instanceTests, 0, instanceTestArray.Count);
            // cullingConfig.materialTest.deniedShaderPasses = DecalSystem.s_MaterialDecalPassNames;
            if (rtas != null)
                rtas.ClearInstances();
            else {
                var settings = new RayTracingAccelerationStructure.Settings();
                settings.managementMode = RayTracingAccelerationStructure.ManagementMode.Manual;
                settings.rayTracingModeMask = RayTracingAccelerationStructure.RayTracingModeMask.Everything;
                rtas = new RayTracingAccelerationStructure();
            }
            var cullRes = rtas.CullInstances(ref cullingConfig);
            
            rtas.Build();
            var count = rtas.GetInstanceCount();
            
            _passData.accelerationStructure = rtas;
            _passData.texWidth = camera.pixelWidth;
            _passData.texHeight = camera.pixelHeight;
        }
        public static void Record(PassData passData, ComputeGraphContext context) {
            var cmd = context.cmd;
            CommandBuffer wrappedCmd = cmd.GetWrappedCommandBufferUnsafe();// Some methods of C# reflection.
            wrappedCmd.SetRayTracingShaderPass(testRs, "DXR");
            cmd.SetRayTracingAccelerationStructure(testRs,"_RtScene",passData.accelerationStructure);
            cmd.SetRayTracingVectorParam(testRs, "_WorldSpaceCameraPos", passData.cameraPosition);
            cmd.SetRayTracingMatrixParam(testRs, "_PixelCoordToViewDirWS", passData.pixelToWorldMatrix);
            cmd.SetRayTracingTextureParam(testRs, "_output", passData.outputTexture);
            // var tex = passData.texture;
            cmd.DispatchRays(testRs, "RayGen", (uint)passData.texWidth, (uint)passData.texHeight, 1,null);
        }
    }
}
