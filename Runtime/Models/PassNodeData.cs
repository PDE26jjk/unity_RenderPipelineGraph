using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class PassNodeData : NodeData {
        internal RPGPass m_Pass;

        public Dictionary<string, PortData> inputs = new();
        public Dictionary<string, PortData> outputs = new();
        public static PassNodeData Instance(Type passType) {
            var data = ScriptableObject.CreateInstance<PassNodeData>();
            data.Init(passType);
            return data;
        }
        internal void Init(Type passType) {
            m_Pass = (RPGPass)Activator.CreateInstance(passType);
            this.exposedName = m_Pass.name;
            Type inputType = passType.GetNestedType("Inputs", BindingFlags.Public | BindingFlags.NonPublic);
            if (inputType is not null)
                foreach (var fieldInfo in inputType.GetFields()) {
                    if (fieldInfo.FieldType == typeof(TextureHandle)) {
                        var resourcePortData = new ResourcePortData(this) {
                            name = fieldInfo.Name
                        };
                        this.inputs[fieldInfo.Name] = resourcePortData;
                    }
                }
            Type outputType = passType.GetNestedType("Outputs", BindingFlags.Public | BindingFlags.NonPublic);
            if (outputType is not null)
                foreach (var fieldInfo in outputType.GetFields()) {
                    if (fieldInfo.FieldType == typeof(TextureHandle)) {
                        var resourcePortData = new ResourcePortData(this) {
                            name = fieldInfo.Name
                        };
                        this.outputs[fieldInfo.Name] = resourcePortData;
                    }
                }
        }
        public List<PortData> dependencies;
        public RPGPass Pass {
            get => m_Pass;
        }
    }
}
