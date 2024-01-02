using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;


namespace RenderPipelineGraph {
    public class PassNodeView : RPGNodeView {
        PassNodeType m_RGPassType;
        internal readonly ParameterViewModel parameterViewModel;
        public new PassNodeData Model {
            get => (PassNodeData)m_Model;
            set => m_Model = value;
        }
        internal List<RPGParameterView> ParameterViews => m_ParameterContainer.Children().OfType<RPGParameterView>().ToList();
        public PassNodeType RGPassType {
            get => ((PassNodeData)m_Model).Pass.PassType;
        }

        static string UXML = "UXML/RPGPassNode.uxml";
        static string Styles = "Styles/RPGPassNode.uss";
        readonly VisualElement m_FlowInputConnectorContainer;
        readonly VisualElement m_FlowOutputConnectorContainer;
        readonly VisualElement m_ParameterContainer;
        public PassNodeView(PassNodeData model) : base(model, UXMLHelpers.PackageResourcePath + UXML) {
            this.AddStyleSheetPath(Styles);
            m_FlowInputConnectorContainer = this.Q("flow-inputs");
            m_FlowOutputConnectorContainer = this.Q("flow-outputs");
            m_ParameterContainer = this.Q("parameter");

            m_FlowInputConnectorContainer.Add(new DependencePortView(Direction.Input));
            m_FlowOutputConnectorContainer.Add(new DependencePortView(Direction.Output));

            parameterViewModel = new ParameterViewModel(this);
        }
        public DependencePortView FlowInPortView => (DependencePortView)m_FlowInputConnectorContainer.Children().First();
        public DependencePortView FlowOutPortView => (DependencePortView)m_FlowOutputConnectorContainer.Children().First();

        public override void Init() {
            m_ParameterContainer.Clear();
            foreach (RPGParameterView parameterView in parameterViewModel.LoadParameterViews()) {
                this.m_ParameterContainer.Add(parameterView);
            }
            title = Model.exposedName;
        }
        public override void GetCompatiblePorts(List<Port> list, RPGPortView portViewToConnect) {
            if (portViewToConnect.node == this) return;
            if (portViewToConnect is DependencePortView) {
                if (portViewToConnect.node is PassNodeView node) {
                    var portToAdd = portViewToConnect.direction == Direction.Input ? FlowOutPortView : FlowInPortView;
                    switch (portViewToConnect.direction) {
                        case Direction.Input when !node.flowOutFlatten.Contains(this):
                        case Direction.Output when !node.flowInFlatten.Contains(this):
                            list.Add(portToAdd);
                            break;
                    }
                }
            }
            else if (portViewToConnect.direction == Direction.Output) {
                foreach (RPGParameterView parameterView in ParameterViews) {
                    if (RPGParameterData.CompatibleResources[parameterView.PortView.portType].Contains(portViewToConnect.portType)) {
                        list.Add(parameterView.PortView);
                    }
                }
            }
        }
        bool m_NeedUpdateDependenciesFlatten = true;
        HashSet<PassNodeView> flowInFlatten = new();
        HashSet<PassNodeView> flowOutFlatten = new();

        public void NotifyFlowInChange(Edge edge, bool add) {
            m_NeedUpdateDependenciesFlatten = true;
            this.NotifyDependenceChangeVM(edge.output as DependencePortView, add);
        }
        public void NotifyFlowOutChange(Edge edge, bool add) {
            m_NeedUpdateDependenciesFlatten = true;
        }
        public override void NotifyPortDraggingStart(Port port) {
            if (m_NeedUpdateDependenciesFlatten) {
                switch (port.direction) {
                    case Direction.Input:
                        UpdateFlowOutFlatten();
                        break;
                    case Direction.Output:
                        UpdateFlowInFlatten();
                        break;
                }
            }
        }

        private void UpdateFlowInFlatten() {
            m_NeedUpdateDependenciesFlatten = false;
            flowInFlatten.Clear();
            Stack<PassNodeView> nodes = new();
            nodes.Push(this);
            int i = 0;
            for (; nodes.Count > 0 && i < kMaxDependenciesFlatten; i++) {
                var node = nodes.Pop();
                foreach (var connection in node.FlowInPortView.connections) {
                    var dependenceNode = connection.output.node as PassNodeView;
                    flowInFlatten.Add(dependenceNode);
                    nodes.Push(dependenceNode);
                }
            }
            if (i == kMaxDependenciesFlatten) {
                Debug.LogError("Too many nodes!");
            }
            // Debug.Log("UpdateDependenciesFlatten " + flowInFlatten.Count);
        }
        public override void OnDelete() {
            foreach (RPGParameterView parameterView in this.m_ParameterContainer.Children().OfType<RPGParameterView>()) {
                parameterView.PortView.DisconnectAll();
            }
            FlowInPortView.DisconnectAll();
            FlowOutPortView.DisconnectAll();
            this.NotifyDeleteVM();
        }
        private void UpdateFlowOutFlatten() {
            m_NeedUpdateDependenciesFlatten = false;
            flowOutFlatten.Clear();
            Stack<PassNodeView> nodes = new();
            nodes.Push(this);
            int i = 0;
            for (; nodes.Count > 0 && i < kMaxDependenciesFlatten; i++) {
                var node = nodes.Pop();
                foreach (var connection in node.FlowOutPortView.connections) {
                    var dependenceNode = connection.input.node as PassNodeView;
                    flowOutFlatten.Add(dependenceNode);
                    nodes.Push(dependenceNode);
                }
            }
            if (i == kMaxDependenciesFlatten) {
                Debug.LogError("Too many nodes!");
            }
            // Debug.Log("UpdateDependenciesFlatten " + flowInFlatten.Count);
        }
        const int kMaxDependenciesFlatten = 2000;

    }
}
