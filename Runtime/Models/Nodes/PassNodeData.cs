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
        // class PassNodeDataBinding : RPGModelBinding {
        //     PassNodeData m_Data;
        //     public int b;
        //     internal PassNodeDataBinding(PassNodeData data) {
        //         m_Data = data;
        //     }
        // }
        internal PassNodeData() {
            // m_ObjBinding = new PassNodeDataBinding(this);
        }
        // public override RPGModelBinding getInspectorBinding() {
        //     return m_ObjBinding;
        // }

        [SerializeField]
        List<JsonData<RPGParameterData>> m_Parameters = new();
        public Dictionary<string, RPGParameterData> Parameters = new();

        internal RPGPass m_Pass;

        internal bool inited = false;

        public static PassNodeData Instance(Type passType) {
            var data = new PassNodeData();
            data.Init(passType);
            return data;
        }

        [SerializeField]
        string m_PassClassName;
        public override void OnBeforeSerialize() {
            m_PassClassName = m_Pass.GetType().FullName;
            foreach (var para in m_Parameters) {
                para.OnBeforeSerialize();
            }
            base.OnBeforeSerialize();
        }
        // https://stackoverflow.com/questions/1825147/type-gettypenamespace-a-b-classname-returns-null
        static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
        public override void OnAfterMultiDeserialize(string json) {
            base.OnAfterMultiDeserialize(json);
            if (m_PassClassName != null) {
                var passType = GetType(m_PassClassName);
                if (passType != null) {
                    Init(passType);
                    return;
                }
            }
            Debug.LogError($"Class {m_PassClassName} not found!");
        }
        internal void Init(Type passType) {
            if (inited) return;
            inited = true;
            Parameters.Clear();
            m_Pass = (RPGPass)Activator.CreateInstance(passType);
            this.exposedName = m_Pass.Name;
            Type PassInputType = passType.GetNestedType("PassData", BindingFlags.Public);
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
                        Debug.LogError("param lost:" + fieldInfo.Name + " l:" + m_Parameters.Count);
                    }
                    needCreate = true;
                }
                if (needCreate || parameterData == null) {
                    parameterData = RPGParameterData.Instance(fieldInfo);
                    if (parameterData == null)
                        continue;
                    m_Parameters.Add(parameterData);
                }
                parameterData.passTypeFieldInfo = fieldInfo;
                parameterData.Init();
                this.Parameters[fieldInfo.Name] = parameterData;
            }

        }
        public List<JsonRef<PassNodeData>> dependencies = new();
        public RPGPass Pass {
            get => m_Pass;
        }
    }
}
