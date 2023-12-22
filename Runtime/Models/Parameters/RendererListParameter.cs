using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class RendererListParameterData : RPGParameterData {
        public bool cullingWhenEmpty;
        internal RendererListParameterData() {
            m_Port = new ResourcePortData(this); 
            ((ResourcePortData)m_Port.value).resourceType = ResourceType.RendererList;
        }
        internal TextureData m_TextureData;
    }
}
