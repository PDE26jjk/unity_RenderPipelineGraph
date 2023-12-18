using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public enum ResourceType {
        Texture,
        Buffer,
        AccelerationStructure,
    }
    public enum UseType {
        Imported,
        Default,
        Shared,
    }
    public class ResourceData : RPGModel {
        public ResourceType type;
        public UseType useType;
        public string name;
    }
    public class BufferData : ResourceData {
        public BufferData() {
            type = ResourceType.Buffer;
        }
        [NonSerialized]
        public BufferDesc desc;
        [NonSerialized]
        public BufferHandle handle;
        [NonSerialized]
        public GraphicsBuffer graphicsBuffer;
    }
    public class TextureData : ResourceData {
        public TextureData() {
            type = ResourceType.Texture;
        }
        [NonSerialized]
        public TextureDesc desc;
        [NonSerialized]
        public TextureHandle handle;
        
        [NonSerialized]
        public RTHandle rtHandle;
    }

    public class RTAData : ResourceData {
        public RTAData() {
            type = ResourceType.AccelerationStructure;
        }
        // [NonSerialized]
        // public RayTracingAccelerationStructureDesc desc;
        [NonSerialized]
        public RayTracingAccelerationStructureHandle handle;
        [NonSerialized]
        public RayTracingAccelerationStructure accelStruct;
    }
}
