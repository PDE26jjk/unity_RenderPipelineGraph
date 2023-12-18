using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public abstract class ResourceNodeData : NodeData {
        ResourceData m_Resource;
        public ResourceData Resource {
            get => m_Resource;
            set => SetResource(value);
        }
        public ResourcePortData attachTo;
        public virtual void SetResource(ResourceData value) {
            m_Resource = value;
            attachTo = new ResourcePortData(this);
            attachTo.resourceType = this.m_Resource.type;
            attachTo.name = "Attach To";
        }
    }
    public class TextureNodeData : ResourceNodeData {
        public static TextureNodeData Instance(TextureData textureData) {
            var data = ScriptableObject.CreateInstance<TextureNodeData>();
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
