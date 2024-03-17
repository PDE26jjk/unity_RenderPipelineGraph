using System;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Runtime.RenderHelper;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    internal static class ShaderPropertyId {

        public static readonly int time = Shader.PropertyToID("_Time");
        public static readonly int sinTime = Shader.PropertyToID("_SinTime");
        public static readonly int cosTime = Shader.PropertyToID("_CosTime");
        public static readonly int deltaTime = Shader.PropertyToID("unity_DeltaTime");
        public static readonly int timeParameters = Shader.PropertyToID("_TimeParameters");
        public static readonly int lastTimeParameters = Shader.PropertyToID("_LastTimeParameters");

        public static readonly int scaledScreenParams = Shader.PropertyToID("_ScaledScreenParams");
        public static readonly int worldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
        public static readonly int screenParams = Shader.PropertyToID("_ScreenParams");
        public static readonly int alphaToMaskAvailable = Shader.PropertyToID("_AlphaToMaskAvailable");
        public static readonly int projectionParams = Shader.PropertyToID("_ProjectionParams");
        public static readonly int zBufferParams = Shader.PropertyToID("_ZBufferParams");
        public static readonly int orthoParams = Shader.PropertyToID("unity_OrthoParams");
        public static readonly int globalMipBias = Shader.PropertyToID("_GlobalMipBias");

        public static readonly int screenSize = Shader.PropertyToID("_ScreenSize");
        public static readonly int screenCoordScaleBias = Shader.PropertyToID("_ScreenCoordScaleBias");
        public static readonly int screenSizeOverride = Shader.PropertyToID("_ScreenSizeOverride");

        public static readonly int viewMatrix = Shader.PropertyToID("unity_MatrixV");
        public static readonly int projectionMatrix = Shader.PropertyToID("glstate_matrix_projection");
        public static readonly int viewAndProjectionMatrix = Shader.PropertyToID("unity_MatrixVP");

        public static readonly int inverseViewMatrix = Shader.PropertyToID("unity_MatrixInvV");
        public static readonly int inverseProjectionMatrix = Shader.PropertyToID("unity_MatrixInvP");
        public static readonly int inverseViewAndProjectionMatrix = Shader.PropertyToID("unity_MatrixInvVP");

        public static readonly int cameraProjectionMatrix = Shader.PropertyToID("unity_CameraProjection");
        public static readonly int inverseCameraProjectionMatrix = Shader.PropertyToID("unity_CameraInvProjection");
        public static readonly int worldToCameraMatrix = Shader.PropertyToID("unity_WorldToCamera");
        public static readonly int cameraToWorldMatrix = Shader.PropertyToID("unity_CameraToWorld");

        public static readonly int previousViewProjectionNoJitter = Shader.PropertyToID("_PrevViewProjMatrix");
        public static readonly int viewProjectionNoJitter = Shader.PropertyToID("_NonJitteredViewProjMatrix");


        // This uniform is specific to the RTHandle system
        public static readonly int rtHandleScale = Shader.PropertyToID("_RTHandleScale");

    }

    public class SetupGlobalConstantPass : RPGPass {
        public class PassData {
            internal CameraData cameraData;
        }
        public override void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
            ((PassData)passData).cameraData = cameraData;
            // Time.renderedFrameCount
        }

        // public override bool Valid() {
        //     return true;
        // }
        public SetupGlobalConstantPass() {
            PassType = PassNodeType.Raster;
            m_AllowPassCulling = false;
            m_AllowGlobalStateModification = true;
        }

        static bool isFirstTimePerFrame = true;

        public static void Record(PassData passData, RasterGraphContext context) {
            bool yFlip = !SystemInfo.graphicsUVStartsAtTop;
            var cmd = context.cmd;
            if (isFirstTimePerFrame) {
                isFirstTimePerFrame = false;
                SetupPerFrameShaderConstants(cmd);
            }
            SetPerCameraShaderVariables(cmd, passData.cameraData, yFlip);

        }

        public override void EndFrame() {
            isFirstTimePerFrame = true;
        }


        static void SetupPerFrameShaderConstants(IBaseCommandBuffer cmd) {
#if UNITY_EDITOR
            float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
#else
            float time = Time.time;
#endif
            float deltaTime = Time.deltaTime;
            float smoothDeltaTime = Time.smoothDeltaTime;
            SetShaderTimeValues(cmd, time, deltaTime, smoothDeltaTime);
        }

        static void SetShaderTimeValues(IBaseCommandBuffer cmd, float time, float deltaTime, float smoothDeltaTime) {
            float timeEights = time / 8f;
            float timeFourth = time / 4f;
            float timeHalf = time / 2f;

            float lastTime = time - deltaTime;

            // Time values
            Vector4 timeVector = time * new Vector4(1f / 20f, 1f, 2f, 3f);
            Vector4 sinTimeVector = new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time));
            Vector4 cosTimeVector = new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time));
            Vector4 deltaTimeVector = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
            Vector4 timeParametersVector = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f);
            Vector4 lastTimeParametersVector = new Vector4(lastTime, Mathf.Sin(lastTime), Mathf.Cos(lastTime), 0.0f);

            cmd.SetGlobalVector(ShaderPropertyId.time, timeVector);
            cmd.SetGlobalVector(ShaderPropertyId.sinTime, sinTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.cosTime, cosTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.deltaTime, deltaTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.timeParameters, timeParametersVector);
            cmd.SetGlobalVector(ShaderPropertyId.lastTimeParameters, lastTimeParametersVector);
        }
        // from URP
        static void SetPerCameraShaderVariables(RasterCommandBuffer cmd, CameraData cameraData, bool isTargetFlipped) {

            Camera camera = cameraData.camera;
            float scaledCameraWidth = camera.pixelWidth;
            float scaledCameraHeight = camera.pixelHeight;
            float cameraWidth = (float)camera.pixelWidth;
            float cameraHeight = (float)camera.pixelHeight;

            if (camera.allowDynamicResolution) {
                scaledCameraWidth *= ScalableBufferManager.widthScaleFactor;
                scaledCameraHeight *= ScalableBufferManager.heightScaleFactor;
            }

            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;
            float invNear = Mathf.Approximately(near, 0.0f) ? 0.0f : 1.0f / near;
            float invFar = Mathf.Approximately(far, 0.0f) ? 0.0f : 1.0f / far;
            float isOrthographic = camera.orthographic ? 1.0f : 0.0f;

            float zc0 = 1.0f - far * invNear;
            float zc1 = far * invNear;

            Vector4 zBufferParams = new Vector4(zc0, zc1, zc0 * invFar, zc1 * invFar);

            if (SystemInfo.usesReversedZBuffer) {
                zBufferParams.y += zBufferParams.x;
                zBufferParams.x = -zBufferParams.x;
                zBufferParams.w += zBufferParams.z;
                zBufferParams.z = -zBufferParams.z;
            }

            // Projection flip sign logic is very deep in GfxDevice::SetInvertProjectionMatrix
            // This setup is tailored especially for overlay camera game view
            // For other scenarios this will be overwritten correctly by SetupCameraProperties
            float projectionFlipSign = isTargetFlipped ? -1.0f : 1.0f;
            Vector4 projectionParams = new Vector4(projectionFlipSign, near, far, 1.0f * invFar);
            cmd.SetGlobalVector(ShaderPropertyId.projectionParams, projectionParams);

            Vector4 orthoParams = new Vector4(camera.orthographicSize * cameraWidth / cameraHeight, camera.orthographicSize, 0.0f, isOrthographic);

            // Camera and Screen variables as described in https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, camera.transform.position);
            cmd.SetGlobalVector(ShaderPropertyId.screenParams, new Vector4(cameraWidth, cameraHeight, 1.0f + 1.0f / cameraWidth, 1.0f + 1.0f / cameraHeight));
            cmd.SetGlobalVector(ShaderPropertyId.scaledScreenParams,
                new Vector4(scaledCameraWidth, scaledCameraHeight, 1.0f + 1.0f / scaledCameraWidth, 1.0f + 1.0f / scaledCameraHeight));
            cmd.SetGlobalVector(ShaderPropertyId.zBufferParams, zBufferParams);
            cmd.SetGlobalVector(ShaderPropertyId.orthoParams, orthoParams);

            cmd.SetGlobalVector(ShaderPropertyId.screenSize, new Vector4(scaledCameraWidth, scaledCameraHeight, 1.0f / scaledCameraWidth, 1.0f / scaledCameraHeight));
            // cmd.SetKeyword(ShaderGlobalKeywords.SCREEN_COORD_OVERRIDE, cameraData.useScreenCoordOverride);
            // cmd.SetGlobalVector(ShaderPropertyId.screenSizeOverride, cameraData.screenSizeOverride);
            // cmd.SetGlobalVector(ShaderPropertyId.screenCoordScaleBias, cameraData.screenCoordScaleBias);

            // { w / RTHandle.maxWidth, h / RTHandle.maxHeight } : xy = currFrame, zw = prevFrame
            cmd.SetGlobalVector(ShaderPropertyId.rtHandleScale, Vector4.one);

            // Calculate a bias value which corrects the mip lod selection logic when image scaling is active.
            // We clamp this value to 0.0 or less to make sure we don't end up reducing image detail in the downsampling case.
            float mipBias = Math.Min((float)-Math.Log(cameraWidth / scaledCameraWidth, 2.0f), 0.0f);
            // Temporal Anti-aliasing can use negative mip bias to increase texture sharpness and new information for the jitter.
            // float taaMipBias = Math.Min(cameraData.taaSettings.mipBias, 0.0f);
            // mipBias = Math.Min(mipBias, taaMipBias);
            cmd.SetGlobalVector(ShaderPropertyId.globalMipBias, new Vector2(mipBias, Mathf.Pow(2.0f, mipBias)));


            SetCameraMatrices(cmd, cameraData, true, isTargetFlipped);
        }

        internal static void SetCameraMatrices(RasterCommandBuffer cmd, CameraData cameraData, bool setInverseMatrices, bool isTargetFlipped) {

            Camera camera = cameraData.camera;
            // NOTE: the URP default main view/projection matrices are the CameraData view/projection matrices.
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;

            Matrix4x4 jitterMat = TAAHelper.jitterMat(camera);
            Matrix4x4 projectionMatrix =jitterMat * camera.projectionMatrix; // Jittered, non-gpu
            // Set the default view/projection, note: projectionMatrix will be set as a gpu-projection (gfx api adjusted) for rendering.
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            

            // Matrix4x4 viewAndProjectionMatrix = projectionMatrix * viewMatrix;
            // cmd.SetGlobalMatrix(ShaderPropertyId.viewMatrix, viewMatrix);
            // cmd.SetGlobalMatrix(ShaderPropertyId.projectionMatrix, projectionMatrix);
            // cmd.SetGlobalMatrix(ShaderPropertyId.viewAndProjectionMatrix, viewAndProjectionMatrix);


            Matrix4x4 gpuProjectionMatrixNoJitter = GL.GetGPUProjectionMatrix(jitterMat *camera.projectionMatrix, true);

            Matrix4x4 vpMatNoJitter = gpuProjectionMatrixNoJitter * viewMatrix;
            cmd.SetGlobalMatrix(ShaderPropertyId.viewProjectionNoJitter, vpMatNoJitter);
            cmd.SetGlobalMatrix(ShaderPropertyId.previousViewProjectionNoJitter, cameraData.previousViewProjectionMatrix);
            cameraData.previousViewProjectionMatrix = vpMatNoJitter;

            if (setInverseMatrices) {

                Matrix4x4 gpuProjectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, isTargetFlipped);
                Matrix4x4 inverseViewMatrix = Matrix4x4.Inverse(viewMatrix);
                Matrix4x4 inverseProjectionMatrix = Matrix4x4.Inverse(gpuProjectionMatrix);
                Matrix4x4 inverseViewProjection = inverseViewMatrix * inverseProjectionMatrix;


                Matrix4x4 worldToCameraMatrix = Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f)) * viewMatrix;
                Matrix4x4 cameraToWorldMatrix = worldToCameraMatrix.inverse;
                cmd.SetGlobalMatrix(ShaderPropertyId.worldToCameraMatrix, worldToCameraMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.cameraToWorldMatrix, cameraToWorldMatrix);

                cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, inverseViewMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.inverseProjectionMatrix, inverseProjectionMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, inverseViewProjection);
            }

        }

    }
}
