using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    
public enum PassNodeType {
    Legacy,
    Unsafe,
    Raster,
    Compute
}
    public abstract class RPGPass : RPGModel{

        string m_Name;
        public PassNodeType PassType { get; protected set; }
        public string name {
            get => m_Name ?? GetType().Name;
            protected set => m_Name = value;
        }
        public static void Run(Inputs inputs, Outputs outputs) {
            throw new Exception(nameof(RPGPass) + " no impl");
        }
        public class Inputs {
            public TextureHandle t1;
            public TextureHandle t2;
        }
        public class Outputs {
            public TextureHandle t3;
            public TextureHandle t4;
        }
    }
}
