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
        public virtual bool Valid() {
            return true;
        }
        public virtual void LoadData(object passData,Camera camera) {
        }
        public PassNodeType PassType { get; protected set; }
        public string Name {
            get => m_Name ?? GetType().Name;
            protected set => m_Name = value;
        }
    }
}
