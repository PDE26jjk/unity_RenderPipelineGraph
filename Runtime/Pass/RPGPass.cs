using UnityEngine;

namespace RenderPipelineGraph {

    public enum PassNodeType {
        Legacy,
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
        public virtual bool Valid() {
            return true;
        }
        public virtual void LoadData(object passData, Camera camera) {
        }
        public PassNodeType PassType { get; protected set; }
        public string Name {
            get => m_Name ?? GetType().Name;
            protected set => m_Name = value;
        }
        
        public virtual void EndFrame() {
        }
    }
}
