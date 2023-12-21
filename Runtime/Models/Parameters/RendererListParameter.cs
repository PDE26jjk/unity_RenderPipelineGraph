using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class RendererListParameter : RPGParameterData {
        public bool cullingWhenEmpty;
        internal RendererListParameter() {
            m_Port = new ResourcePortData(this); 
            ((ResourcePortData)m_Port.value).resourceType = ResourceType.RendererList;
        }
        internal TextureData m_TextureData;
    }
}
