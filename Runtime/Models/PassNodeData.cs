using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using RenderPipelineGraph.Attribute;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public class PassNodeData : NodeData {
        internal RPGPass m_Pass;

        public Dictionary<string, PortData> Attachments = new();
        public static PassNodeData Instance(Type passType) {
            var data = ScriptableObject.CreateInstance<PassNodeData>();
            data.Init(passType);
            return data;
        }
        internal void Init(Type passType) {
            m_Pass = (RPGPass)Activator.CreateInstance(passType);
            this.exposedName = m_Pass.Name;
            Type PassInputType = passType.GetNestedType("PassData", BindingFlags.Public | BindingFlags.NonPublic);
            if (PassInputType is not null)
                foreach (var fieldInfo in PassInputType.GetFields()) {
                    if (fieldInfo.FieldType == typeof(TextureHandle)) {
                        var customAttributes = fieldInfo.GetCustomAttributes().Select(t=>t.GetType()).ToArray();
                        
                        if ( customAttributes.Contains(typeof(ReadAttribute))
                        ||  customAttributes.Contains(typeof(WriteAttribute))
                        ||  customAttributes.Contains(typeof(FragmentAttribute))
                        ) {
                            var resourcePortData = new ResourcePortData(this) {
                                name = fieldInfo.Name
                            };
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
        public List<PassNodeData> dependencies = new();
        public RPGPass Pass {
            get => m_Pass;
        }
    }
}
