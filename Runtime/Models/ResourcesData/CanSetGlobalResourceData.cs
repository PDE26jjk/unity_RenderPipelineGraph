using UnityEngine;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public abstract class CanSetGlobalResourceData:ResourceData {
        
        [SerializeField]
        bool m_SetGlobalTextureAfterAfterWritten = false;
        public bool SetGlobalTextureAfterAfterWritten {
            get => m_SetGlobalTextureAfterAfterWritten;
            set => m_SetGlobalTextureAfterAfterWritten = value;
        }
        [FormerlySerializedAs("m_ShaderPropertyId")]
        [SerializeField]
        string shaderPropertyIdStr = string.Empty;
        public string ShaderPropertyIdStr {
            get => shaderPropertyIdStr;
            set => shaderPropertyIdStr = value;
        }
    }
}
