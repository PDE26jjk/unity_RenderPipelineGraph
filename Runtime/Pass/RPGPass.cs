#if UNITY_EDITOR
using System.Runtime.CompilerServices;
#endif
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public enum PassNodeType {
        Legacy = 0, // not supported
        Unsafe,
        Raster,
        Compute
    }
    public abstract class RPGPass {

        string m_Name;
        protected bool m_AllowPassCulling = true;
        public bool AllowPassCulling => m_AllowPassCulling;
        protected bool m_AllowGlobalStateModification = false;
        public bool AllowGlobalStateModification => m_AllowGlobalStateModification;
        public virtual void PipelineCreate() { }
        public virtual void PipelineDestroy() { }
        public virtual bool Valid(CameraData cameraData) {
            return true;
        }
        public virtual void Setup(object passData, CameraData cameraData, RenderGraph renderGraph, IBaseRenderGraphBuilder builder) {
        }
        public PassNodeType PassType { get; protected set; }
        public string Name {
            get => m_Name ?? GetType().Name;
            protected set => m_Name = value;
        }

        public virtual void EndFrame() {
        }
        protected RPGPass(
            PassNodeType passType = PassNodeType.Raster
#if UNITY_EDITOR // for debug in render graph viewer
            , [CallerFilePath] string filePath = ""
#endif
        ) {
#if UNITY_EDITOR
            this.filePath = filePath;
#endif
            PassType = passType;
        }
#if UNITY_EDITOR
        public string filePath = "";
#endif
    }
}
