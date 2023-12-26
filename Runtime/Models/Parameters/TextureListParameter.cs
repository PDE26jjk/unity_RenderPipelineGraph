using System;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TextureListParameterData : RPGParameterData {
        static readonly int kMaxListSize = 10;
        public int listSize;
        public bool[] read = new bool[kMaxListSize];
        public bool[] write = new bool[kMaxListSize];
        public Tuple<bool, int>[] fragment = new Tuple<bool, int>[kMaxListSize];
        public int depth = -1;
        public bool[] randomAccess;
        internal TextureListParameterData(FieldInfo fieldInfo) : base(fieldInfo) {
            m_Port.value.resourceType = ResourceType.TextureList;
        }
        public override void Init() {
            // base.Init();
            var listSizeAttribute = passTypeFieldInfo.GetCustomAttribute<ListSizeAttribute>();
            if (listSizeAttribute is null || listSizeAttribute.size <= 0 || listSizeAttribute.size > kMaxListSize) {
                throw new ArgumentNullException($"Texture List {passTypeFieldInfo.Name} need a LiseSize Attribute with 0 < size <= {kMaxListSize}");
            }
            this.listSize = listSizeAttribute.size;
            var readAttribute = passTypeFieldInfo.GetCustomAttribute<ReadAttribute>() ?? new();
            var writeAttribute = passTypeFieldInfo.GetCustomAttribute<WriteAttribute>() ?? new();
            var fragmentAttributes = passTypeFieldInfo.GetCustomAttributes<FragmentAttribute>().ToDictionary(t => t.listIndex, t => t.index);
            var depthAttribute = passTypeFieldInfo.GetCustomAttribute<DepthAttribute>();
            depth = depthAttribute?.listIndex ?? -1;
            for (int i = 0; i < listSize; i++) {
                read[i] = readAttribute.listIndex.Contains(i);
                write[i] = writeAttribute.listIndex.Contains(i);
                if (fragmentAttributes.TryGetValue(i, out int attribute)) {
                    fragment[i] = new Tuple<bool, int>(true, attribute);
                }
                else {
                    fragment[i] = new Tuple<bool, int>(false, -1);
                }
            }
        }
        
        public override void LoadDataField(object passData, IBaseRenderGraphBuilder builder) {
            if (GetValue() is not TextureData textureData) {
                Debug.LogError($"texture error: {Name} cannot load.");
                return;
            }
            passTypeFieldInfo.SetValue(passData, textureData.handle);
            // if (depth) {
            //     (builder as IRasterRenderGraphBuilder)?.SetRenderAttachmentDepth(textureData.handle);
            // }
            // else if (fragment) {
            //     (builder as IRasterRenderGraphBuilder)?.SetRenderAttachment(textureData.handle, 0);
            // }
            // else if (read || write) {
            //     AccessFlags flag = AccessFlags.None;
            //     if (read) flag |= AccessFlags.Read;
            //     if (write) flag |= AccessFlags.Write;
            //     builder.UseTexture(textureData.handle, flag);
            // }
            // // Set Global Texture After Write.
            // if (write || depth || fragment) {
            //     if (textureData.usage == Usage.Created && textureData.SetGlobalTextureAfterAfterWritten && textureData.ShaderPropertyIdStr != string.Empty) {
            //         builder.SetGlobalTextureAfterPass(textureData.handle, RenderGraphUtils.GetShaderPropertyId(textureData.ShaderPropertyIdStr));
            //     }
            // }
        }
        TextureListParameterData(){}
    }
}
