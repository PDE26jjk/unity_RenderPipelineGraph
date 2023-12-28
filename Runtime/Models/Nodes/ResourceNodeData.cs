using System.Collections.Generic;
using RenderPipelineGraph.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class ResourceNodeData : NodeData {
        [SerializeField]
        JsonRef<ResourceData> m_Resource;
        public ResourceData Resource {
            get => m_Resource;
            set => SetResource(value);
        }
        [SerializeField]
        public JsonData<ResourcePortData> m_AttachTo;
        public ResourcePortData AttachTo {
            get => m_AttachTo.value;
            set => m_AttachTo = value;
        }
        public virtual void SetResource(ResourceData value) {
            m_Resource = value;
            AttachTo ??= new ResourcePortData(this);
            AttachTo.resourceType = this.Resource.type;
            AttachTo.name = "Attach To";
        }
    }
    public class TextureNodeData : ResourceNodeData {
        public static TextureNodeData Instance(TextureData textureData) {
            var data = new TextureNodeData();
            data.SetResource(textureData);
            data.Init();
            return data;
        }
        void Init() {
            this.exposedName = Resource.name;
            // this.desc.name = "ttt";
        }
    }
}
