using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class CullingResultParameterData : RPGParameterData {
        internal CullingResultParameterData() {
            UseDefault = true;
        }
        public override bool NeedPort() {
            return false;
        }
        
    }
}
