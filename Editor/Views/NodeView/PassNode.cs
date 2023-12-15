using System;
using System.Reflection;
using UnityEngine.Rendering.RenderGraphModule;


namespace RenderPipelineGraph {
    public class PassNode : RPGNode {
        PassNodeType m_RGPassType;
        public PassNodeType RGPassType { get => ((PassNodeData)m_Model).Pass.PassType; }
        protected override void Init() {
            base.Init();
            
        }
        public PassNode(PassNodeData model) : base(model) {
            /*m_Pass = new TestPass1();
            m_Model = m_Pass;
            var passType = m_Pass.GetType();
            Type inputType = passType.GetNestedType("Inputs", BindingFlags.Public | BindingFlags.NonPublic);
            if (inputType is not null)
                foreach (var fieldInfo in inputType.GetFields()) {
                    if (fieldInfo.FieldType == typeof(TextureHandle)) {
                        var port = RPGPort.inputPort(typeof(TextureHandle));
                        port.portName = fieldInfo.Name;
                        inputContainer.Add(port);
                    }
                }
            Type outputType = passType.GetNestedType("Outputs", BindingFlags.Public | BindingFlags.NonPublic);
            if (outputType is not null)
                foreach (var fieldInfo in outputType.GetFields()) {
                    if (fieldInfo.FieldType == typeof(TextureHandle)) {
                        var port = RPGPort.outputPort(typeof(TextureHandle));
                        port.portName = fieldInfo.Name;
                        outputContainer.Add(port);
                    }
                }*/
            // Type inputType = passType.GetNestedType("Outputs");
        }
    }
}
