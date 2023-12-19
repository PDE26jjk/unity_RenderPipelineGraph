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
    }
    public enum UseType {
        Default,
        Imported,
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
        // [NonSerialized]
        public BufferDesc desc = new();
        [NonSerialized]
        public BufferHandle handle;
        [NonSerialized]
        public GraphicsBuffer graphicsBuffer; 
    }
    public class RPGTextureDesc:JsonObject {
        ///<summary>Texture sizing mode.</summary>
        public TextureSizeMode sizeMode;
        ///<summary>Texture width.</summary>
        public int width;
        ///<summary>Texture height.</summary>
        public int height;
        ///<summary>Number of texture slices..</summary>
        public int slices;
        ///<summary>Texture scale.</summary>
        public Vector2 scale;
        
        [NonSerialized]
        TextureDesc m_TextureDesc;
        public TextureDesc GetDescStruct() {
            this.m_TextureDesc = new() {
                sizeMode = sizeMode,
                width = width,
                height = height,
                slices = slices,
                scale = scale
            };
            return this.m_TextureDesc;
        }
    }
    public class TextureData : ResourceData {
        public TextureData() {
            type = ResourceType.Texture;
            m_desc = new RPGTextureDesc();
        }
        public override void OnBeforeSerialize() {
            this.m_desc.OnBeforeSerialize();
            base.OnBeforeSerialize();
        }

        public JsonData<RPGTextureDesc> m_desc;
        
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
