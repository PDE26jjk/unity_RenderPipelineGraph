using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class TextureParameterData : RPGParameterData {
        public bool read;
        public bool write;
        public bool fragment;
        public bool depth;
        public bool randomAccess;
        internal TextureParameterData() {
            m_Port = new ResourcePortData(this);
            ((ResourcePortData)m_Port.value).resourceType = ResourceType.Texture;
        }
    }
}
