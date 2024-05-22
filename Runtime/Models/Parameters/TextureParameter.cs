using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public class TextureParameterData : RPGParameterData {
        public bool read;
        public bool write;
        public bool fragment;
        public bool input;
        public bool depth;
        // public bool randomAccess;
        public int listIndex = 0;
        [FormerlySerializedAs("fragmentIndex")]
        public int mrtIndex = 0;

        internal TextureParameterData(FieldInfo fieldInfo) : base(fieldInfo) {
            m_Port.value.resourceType = ResourceType.Texture;
        }

        public override void Init() {
            base.Init();

            if (customAttributes.Contains(typeof(DepthAttribute))) {
                depth = true;
            }
            if (customAttributes.Contains(typeof(InputAttribute))) {
                InputAttribute inputAttribute = passTypeFieldInfo.GetCustomAttributes<InputAttribute>().First();
                input = true;
                mrtIndex = inputAttribute.index;
                read = true;
                write = false;
            }
            else if (customAttributes.Contains(typeof(FragmentAttribute))) {
                FragmentAttribute fragmentAttribute = passTypeFieldInfo.GetCustomAttributes<FragmentAttribute>().First();
                fragment = true;
                mrtIndex = fragmentAttribute.index;
                write = true;
                read = false;
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
            var resourceData = GetValue() as CanSetGlobalResourceData;
            TextureHandle textureHandle;
            if (resourceData is TextureData textureData) {
                textureHandle = textureData.handle;
            }
            else if (resourceData is TextureListData textureListData) {
                textureHandle = textureListData.handles[listIndex];
            }
            else {
                Debug.LogError($"texture error: {Name} cannot load.");
                return;
            }
            passTypeFieldInfo.SetValue(passData, textureHandle);
            if (depth) {
                (builder as IRasterRenderGraphBuilder)?.SetRenderAttachmentDepth(textureHandle);
            }
            else if (fragment) {
                (builder as IRasterRenderGraphBuilder)?.SetRenderAttachment(textureHandle, mrtIndex);
            }
            else if (input) {
                (builder as IRasterRenderGraphBuilder)?.SetInputAttachment(textureHandle, mrtIndex);
            }
            else if (read || write) {
                AccessFlags flag = AccessFlags.None;
                if (read) flag |= AccessFlags.Read;
                if (write) flag |= AccessFlags.Write;
                builder.UseTexture(textureHandle, flag);
            }
            // Set Global Texture After Write.
            if (write || depth || fragment) {
                if (resourceData.usage == Usage.Created && resourceData.SetGlobalTextureAfterAfterWritten && resourceData.ShaderPropertyIdStr != string.Empty) {
                    builder.SetGlobalTextureAfterPass(textureHandle, RenderGraphUtils.GetShaderPropertyId(resourceData.ShaderPropertyIdStr));
                }
            }
        }
        TextureParameterData() {
        }
    }
}
