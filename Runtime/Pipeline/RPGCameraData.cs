using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class CameraData : IDisposable {
        internal readonly RTHandleSystem rtHandleSystem = new();
        internal readonly BufferedRTHandleSystem historyRTHandleSystem = new();
        internal readonly Camera m_Camera;
        RPGRenderer m_Renderer;
        RenderGraph renderGraph;
        internal bool needReloadGraph = true;
        public void Dispose() {
            m_Renderer.Dispose();
            RestoreRTHandels();
            rtHandleSystem.Dispose();
            historyRTHandleSystem.Dispose();
        }
        internal Vector2Int sizeInPixel = Vector2Int.zero;
        static FieldInfo _defaultRTHandlesInstanceInfo;
        static RTHandleSystem _defaultRTHandles;
        public CameraData(Camera cam) {
            this.m_Camera = cam;
            renderGraph = new(cam.name + " RPG");
            this.m_Renderer = new();
        }
        public void Render(RPGAsset asset, ScriptableRenderContext context) {
            m_Renderer.Render(asset,renderGraph,context,this);
            renderGraph.EndFrame();
            needReloadGraph = false;
        }

        public void BorrowRTHandles() {
            _defaultRTHandlesInstanceInfo ??= typeof(RTHandles).GetField("s_DefaultInstance", BindingFlags.Static | BindingFlags.NonPublic);

            _defaultRTHandles ??= _defaultRTHandlesInstanceInfo.GetValue(null) as RTHandleSystem;

            _defaultRTHandlesInstanceInfo?.SetValue(null, this.rtHandleSystem);
        }
        public void RestoreRTHandels() {
            _defaultRTHandlesInstanceInfo?.SetValue(null, _defaultRTHandles);
        }

        public void SwapAndSetReferenceSize() {
            if (sizeInPixel.x != m_Camera.pixelWidth
                || sizeInPixel.y != m_Camera.pixelHeight) {
                sizeInPixel.x = m_Camera.pixelWidth;
                sizeInPixel.y = m_Camera.pixelHeight;
                RTHandles.ResetReferenceSize(sizeInPixel.x, sizeInPixel.y);
                historyRTHandleSystem.SwapAndSetReferenceSize(sizeInPixel.x, sizeInPixel.y);
            }
        }
    }

}
