using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public class PassNodeData : NodeData {
        class PassNodeDataBinding : RPGModelBinding {
            PassNodeData m_Data;
            public int b;
            internal PassNodeDataBinding(PassNodeData data) {
                m_Data = data;
            }
        }
        internal PassNodeData() {
            m_ObjBinding = new PassNodeDataBinding(this);
        }
        public override RPGModelBinding getInspectorBinding() {
            return m_ObjBinding;
        }

        [SerializeField]
        List<JsonData<RPGParameterData>> m_Parameters = new();
        public Dictionary<string, RPGParameterData> Parameters = new();

        internal RPGPass m_Pass;

        public static PassNodeData Instance(Type passType) {
            var data = new PassNodeData();
            data.Init(passType);
            return data;
        }
        [SerializeField]
        string m_PassType;
        public override void OnBeforeSerialize() {
            m_PassType = m_Pass.GetType().FullName;
            foreach (var para in m_Parameters) {
                para.OnBeforeSerialize();
            }
            base.OnBeforeSerialize();
        }
        public override void OnAfterMultiDeserialize(string json) {
            base.OnAfterMultiDeserialize(json);
            if (m_PassType != null) {
                var passType = Type.GetType(m_PassType);
                if (passType != null) {
                    Init(passType);
                    return;
                }
            }
            Debug.LogError("No pass class found.");
        }
        internal void Init(Type passType) {
            Parameters.Clear();
            m_Pass = (RPGPass)Activator.CreateInstance(passType);
            this.exposedName = m_Pass.Name;
            Type PassInputType = passType.GetNestedType("PassData", BindingFlags.Public | BindingFlags.NonPublic);
            if (PassInputType is null)
                return;
            foreach (var fieldInfo in PassInputType.GetFields()) {
                var p = m_Parameters.Where(t => t.value.Name == fieldInfo.Name).ToArray();
                RPGParameterData parameterData = null;
                bool needCreate = false;
                if (p.Length > 0) {
                    parameterData = p[0];
                }
                else {
                    if (MultiJsonInternal.isDeserializing) {
                        Debug.LogError("port lost:" + fieldInfo.Name + " l:" + m_Parameters.Count);
                    }
                    needCreate = true;
                }
                var customAttributes = fieldInfo.GetCustomAttributes().Select(t => t.GetType()).ToArray();
                if (needCreate) {
                    if (fieldInfo.FieldType == typeof(TextureHandle)) {
                        var textureParameter = new TextureParameter() {
                            Name = fieldInfo.Name
                        };
                        parameterData = textureParameter;
                        if (customAttributes.Contains(typeof(FragmentAttribute))) {
                            textureParameter.fragment = true;
                            textureParameter.write = true;
                        }
                        if (customAttributes.Contains(typeof(WriteAttribute))
                        ) {
                            textureParameter.write = true;
                        }
                        if (customAttributes.Contains(typeof(ReadAttribute))
                        ) {
                            textureParameter.read = true;
                        }
                        m_Parameters.Add(parameterData);
                    }else if (fieldInfo.FieldType == typeof(RendererList)) {
                        var rendererListParameter = new RendererListParameter();
                        parameterData = rendererListParameter;
                        
                        if (customAttributes.Contains(typeof(CullingWhenEmptyAttribute))) {
                            rendererListParameter.cullingWhenEmpty = true;
                        }
                    }
                }
                if (customAttributes.Contains(typeof(DefaultAttribute))) {
                    parameterData.UseDefault = true;
                }
                this.Parameters[fieldInfo.Name] = parameterData;
            }

        }
        public List<JsonRef<PassNodeData>> dependencies = new();
        public RPGPass Pass {
            get => m_Pass;
        }
    }
}
