using System;
using System.Reflection;
using RenderPipelineGraph.Runtime.Volumes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class CameraData : IDisposable {
        internal RTHandleSystem rtHandleSystem;
        internal BufferedRTHandleSystem historyRTHandleSystem;
        internal Camera m_Camera;
        public Camera camera => m_Camera;
        RPGRenderer m_Renderer;
        internal RenderGraph renderGraph;
        internal bool needReloadGraph = true;
        public void Dispose() {
            renderGraph?.Cleanup();
            m_Renderer?.Dispose();
            RestoreRTHandels();
            rtHandleSystem?.Dispose();
            historyRTHandleSystem?.Dispose();
        }
        Vector2Int sizeInPixel = Vector2Int.zero;
        public Vector2Int SizeInPixel => sizeInPixel;
        static FieldInfo _defaultRTHandlesInstanceInfo;
        static RTHandleSystem _defaultRTHandles;
        public Matrix4x4 previousViewProjectionMatrix = Matrix4x4.identity;
        void Refresh() {
            Dispose();
            needReloadGraph = true;
            sizeInPixel = Vector2Int.zero;
            rtHandleSystem = new();
            historyRTHandleSystem = new();
            renderGraph = CreateRenderGraph(m_Camera);
            this.m_Renderer = new();
        }
        public CameraData(Camera cam) {
            this.m_Camera = cam;
            Refresh();
        }
        RenderGraph CreateRenderGraph(Camera cam) {
            return new(cam.name + " RPG");
        }
        public void Render(RPGAsset asset, ScriptableRenderContext context) {
            m_Renderer.Render(asset, renderGraph, context, this);
            renderGraph.EndFrame();
            needReloadGraph = false;
        }
        // call it before rendering camera.
        public void BorrowRTHandles() {
            _defaultRTHandlesInstanceInfo ??= typeof(RTHandles).GetField("s_DefaultInstance", BindingFlags.Static | BindingFlags.NonPublic);

            _defaultRTHandles ??= _defaultRTHandlesInstanceInfo.GetValue(null) as RTHandleSystem;

            _defaultRTHandlesInstanceInfo?.SetValue(null, this.rtHandleSystem);
        }
        // call it after rendering camera.
        public void RestoreRTHandels() {
            if (_defaultRTHandles != null)
                _defaultRTHandlesInstanceInfo?.SetValue(null, _defaultRTHandles);
        }

        public void SwapAndSetReferenceSize() {
            if (sizeInPixel.x != m_Camera.pixelWidth
                || sizeInPixel.y != m_Camera.pixelHeight) {
                sizeInPixel.x = m_Camera.pixelWidth;
                sizeInPixel.y = m_Camera.pixelHeight;
                RTHandles.ResetReferenceSize(sizeInPixel.x, sizeInPixel.y);
                historyRTHandleSystem.ResetReferenceSize(sizeInPixel.x, sizeInPixel.y);
            }
            historyRTHandleSystem.SwapAndSetReferenceSize(sizeInPixel.x, sizeInPixel.y);
        }
    }

}
