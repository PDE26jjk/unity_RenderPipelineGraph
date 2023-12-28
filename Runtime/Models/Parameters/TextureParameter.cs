using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TextureParameterData : RPGParameterData {
        public bool read;
        public bool write;
        public bool fragment;
        public bool depth;
        public bool randomAccess;
        public int listIndex;

        internal TextureParameterData(FieldInfo fieldInfo) : base(fieldInfo) {
            m_Port.value.resourceType = ResourceType.Texture;
        }
        
        public override void Init() {
            base.Init();

            if (customAttributes.Contains(typeof(DepthAttribute))) {
                depth = true;
                read = true;
                write = true;
            }
            if (customAttributes.Contains(typeof(FragmentAttribute))) {
                fragment = true;
                write = true;
            }
            if (customAttributes.Contains(typeof(WriteAttribute))
            ) {
                write = true;
            }
            if (customAttributes.Contains(typeof(ReadAttribute))
            ) {
                read = true;
            }
        }

        public override void LoadDataField(object passData, IBaseRenderGraphBuilder builder) {
            if (GetValue() is not TextureData textureData) {
                Debug.LogError($"texture error: {Name} cannot load.");
                return;
            }
            passTypeFieldInfo.SetValue(passData, textureData.handle);
            if (depth) {
                (builder as IRasterRenderGraphBuilder)?.SetRenderAttachmentDepth(textureData.handle);
            }
            else if (fragment) {
                (builder as IRasterRenderGraphBuilder)?.SetRenderAttachment(textureData.handle, 0);
            }
            else if (read || write) {
                AccessFlags flag = AccessFlags.None;
                if (read) flag |= AccessFlags.Read;
                if (write) flag |= AccessFlags.Write;
                builder.UseTexture(textureData.handle, flag);
            }
            // Set Global Texture After Write.
            if (write || depth || fragment) {
                if (textureData.usage == Usage.Created && textureData.SetGlobalTextureAfterAfterWritten && textureData.ShaderPropertyIdStr != string.Empty) {
                    builder.SetGlobalTextureAfterPass(textureData.handle, RenderGraphUtils.GetShaderPropertyId(textureData.ShaderPropertyIdStr));
                }
            }
        }
        TextureParameterData(){}
    }
}
