using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;
using Edge = UnityEditor.Graphs.Edge;


namespace RenderPipelineGraph {
    public class PassNode : RPGNode {
        PassNodeType m_RGPassType;
        public PassNodeType RGPassType {
            get => ((PassNodeData)m_Model).Pass.PassType;
        }

        static string UXML = "UXML/RPGPassNode.uxml";
        static string Styles = "Styles/RPGPassNode.uss";
        readonly VisualElement m_FlowInputConnectorContainer;
        readonly VisualElement m_FlowOutputConnectorContainer;
        public PassNode(PassNodeData model) : base(model, UXMLHelpers.PackageResourcePath + UXML) {
            this.AddStyleSheetPath(Styles);
            m_FlowInputConnectorContainer = this.Q("flow-inputs");
            m_FlowOutputConnectorContainer = this.Q("flow-outputs");

            m_FlowInputConnectorContainer.Add(new DependencePort(Direction.Input));
            m_FlowOutputConnectorContainer.Add(new DependencePort(Direction.Output));
        }
        public DependencePort FlowInPort => (DependencePort)m_FlowInputConnectorContainer.Children().First();
        public DependencePort FlowOutPort => (DependencePort)m_FlowOutputConnectorContainer.Children().First();

        void Init() {

        }
        public override void GetCompatiblePorts(List<Port> list, RPGPort portToConnect) {
            base.GetCompatiblePorts(list, portToConnect);
            if (portToConnect is DependencePort && portToConnect.node != this) {
                if (portToConnect.node is PassNode node) {
                    var portToAdd = portToConnect.direction == Direction.Input ? FlowOutPort : FlowInPort;
                    switch (portToConnect.direction) {
                        case Direction.Input when !node.flowOutFlatten.Contains(this):
                        case Direction.Output when !node.flowInFlatten.Contains(this):
                            list.Add(portToAdd);
                            break;
                    }

                }
            }
        }
        bool m_NeedUpdateDependenciesFlatten = true;
        HashSet<PassNode> flowInFlatten = new();
        HashSet<PassNode> flowOutFlatten = new();
        public void NotifyDependenceChange() {
            Debug.Log("NotifyDependenceChange");
            m_NeedUpdateDependenciesFlatten = true;
            GetFirstAncestorOfType<RPGView>().m_NodeViewModel.UpdateNodeDependence(this);
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
            Stack<PassNode> nodes = new();
            nodes.Push(this);
            int i = 0;
            for (; nodes.Count > 0 && i < kMaxDependenciesFlatten; i++) {
                var node = nodes.Pop();
                foreach (var connection in node.FlowInPort.connections) {
                    var dependenceNode = connection.output.node as PassNode;
                    flowInFlatten.Add(dependenceNode);
                    nodes.Push(dependenceNode);
                }
            }
            if (i == kMaxDependenciesFlatten) {
                Debug.LogError("Too many nodes!");
            }
            // Debug.Log("UpdateDependenciesFlatten " + flowInFlatten.Count);
        }
        private void UpdateFlowOutFlatten() {
            m_NeedUpdateDependenciesFlatten = false;
            flowOutFlatten.Clear();
            Stack<PassNode> nodes = new();
            nodes.Push(this);
            int i = 0;
            for (; nodes.Count > 0 && i < kMaxDependenciesFlatten; i++) {
                var node = nodes.Pop();
                foreach (var connection in node.FlowOutPort.connections) {
                    var dependenceNode = connection.input.node as PassNode;
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
