using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public abstract class RPGParameterData : Slottable {
        // public abstract T Value();
        public override void OnBeforeSerialize() {
            m_Port.OnBeforeSerialize();
            m_DefaultResource.OnBeforeSerialize();
        }
        public ResourceType resourceType => this.m_Port.value.resourceType;
        
        [SerializeField]
        protected JsonRef<ResourceData> m_DefaultResource;
        public ResourceData DefaultResource =>
            m_DefaultResource;
        public void SetDefaultResource(ResourceData resourceData) {
            m_DefaultResource = resourceData;
        }
        public virtual ResourceData GetValue() {
            if (UseDefault) return m_DefaultResource;
            if (Port.LinkTo.Count > 0) {
                return (Port.LinkTo[0].Owner as ResourceNodeData)?.Resource;
            }
            return null;
        }
        [SerializeField]
        string m_Name;
        public string Name {
            get => m_Name;
            set => m_Name = value;
        }
        [SerializeField]
        protected JsonData<ResourcePortData> m_Port;
        public ResourcePortData Port =>
            m_Port;
        public bool UseDefault = false;


        protected RPGParameterData(FieldInfo fieldInfo) {
            passTypeFieldInfo = fieldInfo;
            m_Port = new ResourcePortData(this);
        }
        // For Json
        protected RPGParameterData() {
        }

        // load Attributes
        public virtual void Init() {
            m_Name = passTypeFieldInfo.Name;
            customAttributes ??= passTypeFieldInfo.GetCustomAttributes().Select(t => t.GetType()).ToArray();
            if (customAttributes.Contains(typeof(DefaultAttribute))) {
                UseDefault = true;
            }
        }

        public static RPGParameterData Instance(FieldInfo fieldInfo) {
            Type type = fieldInfo.FieldType;
            RPGParameterData parameterData = type switch {
                not null when type == typeof(TextureHandle) => new TextureParameterData(fieldInfo),
                not null when type == typeof(RendererListHandle) => new RendererListParameterData(fieldInfo),
                not null when type == typeof(CullingResults) => new CullingResultParameterData(fieldInfo),
                not null when type == typeof(List<TextureHandle>) => new TextureListParameterData(fieldInfo),
                _ => null
            };
            return parameterData;
        }

        public virtual bool NeedPort() {
            return true;
        }
        public virtual bool CanConvertTo(Type type) {
            return false;
        }

        #region Runtime

        [NonSerialized]
        public FieldInfo passTypeFieldInfo;
        protected Type[] customAttributes;

        // something like 
        // passTypeFieldInfo.SetValue(passData,GetValue());
        // or
        // passTypeFieldInfo.SetValueDirect(reference,GetValue());
        // and builder.UseXXX
        public abstract void LoadDataField(object passData, IBaseRenderGraphBuilder builder);

        #endregion

    }
}
