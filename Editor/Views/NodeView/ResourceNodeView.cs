using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Editor;
using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Interface;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {

    public partial class ResourceNodeView : RPGNodeView{
        string m_Name;
        protected AttachPortView m_PortView;
        public new ResourceNodeData Model => (ResourceNodeData)base.Model;
        public AttachPortView PortView => m_PortView;
        protected VisualElement m_PortContainer;
        public ResourceType ResourceType => Model.Resource.type;
        static string UXML = "UXML/RPGResourceNode.uxml";
        static string Styles = "Styles/RPGResourceNode.uss";
        public ResourceNodeView(ResourceNodeData model) : base(model, UXMLHelpers.PackageResourcePath + UXML) {
            this.AddStyleSheetPath(Styles);
            m_PortContainer = this.Q("port");
        }
        public override void Init() {
            this.m_PortView ??= new AttachPortView(Direction.Output, Model.Resource.GetType());
            m_PortView.ConnectorText = Model.Resource.name;
            m_PortContainer.Clear();
            this.m_PortContainer.Add(m_PortView);
            // this.title = Enum.GetName(typeof(ResourceType), ResourceType);
        }

        public override void OnDelete() {
            this.m_PortView.DisconnectAll();
            this.NotifyDeleteVM();
        }
        public void NotifyDisconnectAllPort() {
            if (m_PortView.connections.Any())
                this.NotifyDisconnectAllPortVM();
        }
        public override void GetCompatiblePorts(List<Port> list, RPGPortView portViewToConnect) {
            if (portViewToConnect.direction == Direction.Output || portViewToConnect is not AttachPortView) return;
            if (RPGParameterData.CompatibleResources[portViewToConnect.portType].Contains(this.PortView.portType)) {
                list.Add(this.PortView);
            }
        }
        public void NotifyDisconnectPort(Edge edge) {
            this.NotifyDisconnectPortVM(edge.input as AttachPortView);
        }
        RPGBlackboardRow GetBlackboardRow() {
            return ResourceViewModel.GetViewModel(this).TryGetResourceViews(Model.Resource, out var row) ? row : null;
        }

    }

}
