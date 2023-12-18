using System;
using UnityEngine.Rendering.RenderGraphModule;
using Object = UnityEngine.Object;

namespace RenderPipelineGraph {

    public enum PassNodeType {
        Legacy,
        Unsafe,
        Raster,
        Compute
    }
    public abstract class RPGPass : RPGModel {

        string m_Name;
        public virtual bool Valid() {
            return true;
        }
        public virtual void LoadData(Object passData) {
        }
        public PassNodeType PassType { get; protected set; }
        public string Name {
            get => m_Name ?? GetType().Name;
            protected set => m_Name = value;
        }
    }
}
