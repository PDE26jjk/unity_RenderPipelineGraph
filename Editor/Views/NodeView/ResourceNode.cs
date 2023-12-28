using System;
using System.Reflection;
using RenderPipelineGraph.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {

    public class ResourceNodeView : RPGNodeView {
        string m_Name;
        protected AttachPortView m_PortView;
        public new ResourceNodeData Model => (ResourceNodeData)base.Model;
        public AttachPortView PortView => m_PortView;
        protected VisualElement m_PortContainer;
        public ResourceType ResourceType => Model.Resource.type;
        static string UXML = "UXML/RPGResourceNode.uxml";
        static string Styles = "Styles/RPGResourceNode.uss"; 
        public ResourceNodeView(ResourceNodeData model) : base(model,UXMLHelpers.PackageResourcePath + UXML) {
            this.AddStyleSheetPath(Styles);
            m_PortContainer = this.Q("port");
        }
        public override void Init() {
            // if(!Contains(m_PortView)) Add(m_PortView);
            this.m_PortView = new AttachPortView(Direction.Output, Model.Resource.GetType());
            m_PortView.ConnectorText = Model.Resource.name;
            this.m_PortContainer.Add(m_PortView);
            // this.title = Enum.GetName(typeof(ResourceType), ResourceType);
        }
    }

    // public class TextureNodeView : ResourceNodeView {
    //     public TextureNodeView(ResourceNodeData model) : base(model) {
    //         title = "Texture";
    //         this.m_PortView = new AttachPortView(Direction.Output, typeof(TextureData));
    //     }
    // }
    // public class BufferNodeView : ResourceNodeView {
    //     public BufferNodeView(ResourceNodeData model) : base(model) {
    //         
    //
    //     }
    // }
}
