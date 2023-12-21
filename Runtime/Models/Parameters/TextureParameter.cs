using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class TextureParameter : RPGParameterData {
        public bool read;
        public bool write;
        public bool fragment;
        public bool depth;
        public bool randomAccess;
        internal TextureParameter() {
            m_Port = new ResourcePortData(this);
            ((ResourcePortData)m_Port.value).resourceType = ResourceType.Texture;
        }
        internal TextureData m_TextureData;
    }
}
