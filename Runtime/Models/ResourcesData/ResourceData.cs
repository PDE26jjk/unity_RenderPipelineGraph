using System;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {

    public enum ResourceType {
        Texture,
        Buffer,
        AccelerationStructure,
        RendererList,
    }
    public enum Usage {
        Created,
        Imported,
        Shared,
    }
    public class ResourceData : RPGModel {
        public ResourceType type;
        [FormerlySerializedAs("useType")]
        public Usage usage;
        public string name;
        public bool isDefault;
    }
    public class BufferData : ResourceData {
        public BufferData() {
            type = ResourceType.Buffer;
        }
        // [NonSerialized]
        public BufferDesc desc = new();
        [NonSerialized]
        public BufferHandle handle;
        [NonSerialized]
        public GraphicsBuffer graphicsBuffer; 
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
