using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public class PassNodeData : NodeData {
        class PassNodeDataBinding : RPGModelBinding {
            public int b;
        }
        PassNodeDataBinding m_Obj;
        public override RPGModelBinding getInspectorBinding() {
            m_Obj ??= new();
            return m_Obj;
        }
        internal RPGPass m_Pass;

        [NonSerialized]
        public Dictionary<string, ResourcePortData> Attachments = new();
        [SerializeField]
        List<JsonData<ResourcePortData>> m_Attachments = new();
        public static PassNodeData Instance(Type passType) {
            var data = new PassNodeData();
            data.Init(passType);
            return data;
        }
        [SerializeField]
        string m_PassType;
        public override void OnBeforeSerialize() {
            m_PassType = m_Pass.GetType().FullName;
            foreach (var port in m_Attachments) {
                port.OnBeforeSerialize();
            }
            base.OnBeforeSerialize();
        }
        public override void OnAfterDeserialize(string json) {
            base.OnAfterDeserialize(json);
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
            Attachments.Clear();
            m_Pass = (RPGPass)Activator.CreateInstance(passType);
            this.exposedName = m_Pass.Name;
            Type PassInputType = passType.GetNestedType("PassData", BindingFlags.Public | BindingFlags.NonPublic);
            if (PassInputType is not null)
                foreach (var fieldInfo in PassInputType.GetFields()) {
                    if (fieldInfo.FieldType == typeof(TextureHandle)) {
                        var customAttributes = fieldInfo.GetCustomAttributes().Select(t => t.GetType()).ToArray();

                        if (customAttributes.Contains(typeof(ReadAttribute))
                            || customAttributes.Contains(typeof(WriteAttribute))
                            || customAttributes.Contains(typeof(FragmentAttribute))
                        ) {
                            var a = m_Attachments.Where(t => t.value.name == fieldInfo.Name).ToArray();
                            ResourcePortData resourcePortData = null;
                            if (a.Length > 0) {
                                resourcePortData = a[0];
                            }
                            else {
                                resourcePortData = new ResourcePortData(this) {
                                    name = fieldInfo.Name
                                };
                                m_Attachments.Add(resourcePortData);
                            }
                            this.Attachments[fieldInfo.Name] = resourcePortData;
                        }
                        // else if (fieldInfo.GetCustomAttribute<WriteAttribute>() is not null) {
                        //     var resourcePortData = new ResourcePortData(this) {
                        //         name = fieldInfo.Name
                        //     };
                        //     this.outputs[fieldInfo.Name] = resourcePortData;
                        // }
                    }
                }

        }
        public List<JsonRef<PassNodeData>> dependencies = new();
        public RPGPass Pass {
            get => m_Pass;
        }
    }
}
