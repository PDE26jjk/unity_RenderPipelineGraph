using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class ResourceNode : RPGNode {
        string m_Name;
        ResourceType m_RGResourceTypeType;
        public ResourceType RGResourceTypeType { get; protected set; }
        public ResourceNode(ResourceNodeData model) : base(model) {
        }
    }
    public class TextureNode : ResourceNode {
        public TextureNode(ResourceNodeData model) : base(model) {
            title = "Texture";
            // Port port1 = RPGPort.InputPort(typeof(TextureHandle));
            // port1.portName = "In";
            // Port port2 = RPGPort.OutputPort(typeof(TextureHandle));
            // port2.portName = "Out";
            // RGResourceTypeType = ResourceType.Texture;
            // inputContainer.Add(port1);
            // outputContainer.Add(port2);
            // m_Model = new TextureNodeData() as TextureNodeData;
            // m_Model.name = string.Empty;
        }
    }
    public class BufferNode : ResourceNode {
        public BufferNode(ResourceNodeData model) : base(model) {
            // title = "Buffer";
            // Port port1 = RPGPort.InputPort(typeof(BufferHandle));
            // port1.portName = "In";
            // Port port2 = RPGPort.OutputPort(typeof(BufferHandle));
            // port2.portName = "Out";
            // RGResourceTypeType = ResourceType.Buffer;
            // inputContainer.Add(port1);
            // outputContainer.Add(port2);
        }
    }
}

