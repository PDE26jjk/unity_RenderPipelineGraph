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
        CullingResult,
        TextureList,
        Count
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
        public string category = string.Empty;
        public bool isDefault => category == "Default";
    }
    public class BufferData : CanSetGlobalResourceData {
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

    public class RTAData : CanSetGlobalResourceData {
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
