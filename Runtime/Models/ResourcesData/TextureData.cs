using System;
using System.Collections.Generic;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class RPGTextureDesc:JsonObject {
        public string name;
        ///<summary>Texture sizing mode.</summary>
        public TextureSizeMode sizeMode;
        ///<summary>Texture width.</summary>
        public int width;
        ///<summary>Texture height.</summary>
        public int height;
        ///<summary>Number of texture slices..</summary>
        public int slices = 1;
        ///<summary>Texture scale.</summary>
        public Vector2 scale;
        
        public GraphicsFormat colorFormat;
        
        public DepthBits depthBufferBits;

        public FilterMode filterMode;
        
        public TextureDimension dimension = TextureDimension.Tex2D;

        public bool clearBuffer;

        public Color clearColor;

        public bool enableRandomWrite;
        
        public bool isShadowMap;

        public MSAASamples msaaSamples = MSAASamples.None;
        
        [NonSerialized]
        TextureDesc m_TextureDesc;
        public TextureDesc GetDescStruct() {
            this.m_TextureDesc = new() {
                name = name,
                sizeMode = sizeMode,
                width = width,
                height = height,
                slices = slices,
                scale = scale,
                colorFormat = colorFormat,
                depthBufferBits = depthBufferBits,
                filterMode = filterMode,
                dimension = dimension,
                clearBuffer = clearBuffer,
                clearColor = clearColor,
                enableRandomWrite = enableRandomWrite,
                msaaSamples = msaaSamples,
                isShadowMap = isShadowMap
            };
            return this.m_TextureDesc;
        }
    }
    public class TextureData : CanSetGlobalResourceData {
        
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
    public enum RPGBuildInRTType {
        CameraTarget = (int)BuiltinRenderTextureType.CameraTarget,
        CameraDepth = (int)BuiltinRenderTextureType.Depth,
    }
    public class BuildInRenderTextureData : TextureData {
        BuildInRenderTextureData(RPGBuildInRTType textureType) {
            usage = Usage.Imported;
            this.textureType = textureType;
            // m_desc = null;
        }
        public readonly RPGBuildInRTType textureType;
        public static readonly Dictionary<RPGBuildInRTType, BuildInRenderTextureData> buildInRenderTextureDatas = new();
        public static BuildInRenderTextureData GetTexture(RPGBuildInRTType textureType) {
            if (!buildInRenderTextureDatas.TryGetValue(textureType, out var buildInRenderTextureData)) {
                buildInRenderTextureDatas[textureType] = buildInRenderTextureData = new(textureType);
                buildInRenderTextureData.name = Enum.GetName(typeof(RPGBuildInRTType), textureType);
            }
            return buildInRenderTextureData;
        }

        BuildInRenderTextureData(){}
        // public BuiltinRenderTextureType textureType = BuiltinRenderTextureType.None;
    }

}
