using System;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
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
        
        public GraphicsFormat colorFormat;
        
        public DepthBits depthBufferBits;
        
        [NonSerialized]
        TextureDesc m_TextureDesc;
        public TextureDesc GetDescStruct() {
            this.m_TextureDesc = new() {
                sizeMode = sizeMode,
                width = width,
                height = height,
                slices = slices,
                scale = scale,
                colorFormat = colorFormat,
                depthBufferBits = depthBufferBits
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

}
