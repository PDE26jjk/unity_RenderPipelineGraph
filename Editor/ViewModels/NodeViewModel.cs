using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class NodeViewModel {
        public static bool GetValidViewModel(VisualElement visualElement, out NodeViewModel nodeViewModel) {
            var graphView = visualElement.GetFirstAncestorOfType<RPGView>();
            nodeViewModel = graphView?.m_NodeViewModel;
            return !(nodeViewModel is null || nodeViewModel.Loading);
        }
        internal Dictionary<NodeData, RPGNodeView> m_NodeViews = new();
        RPGView m_GraphView;
        public RPGView GraphView => m_GraphView;
        public RPGGraphData currentGraph => m_GraphView.currentGraph;
        public NodeViewModel(RPGView graphView) {
            this.m_GraphView = graphView;
        }
        public bool GetNodeView(NodeData nodeData, out RPGNodeView nodeViewView) {
            return m_NodeViews.TryGetValue(nodeData, out nodeViewView);
        }
        internal bool Loading = false;

        void InitDependenceEdge() {
            foreach (var keyValuePair in m_NodeViews) {
                NodeData nodeData = keyValuePair.Key;
                RPGNodeView nodeView = keyValuePair.Value;
                if (nodeData is not PassNodeData passNodeData) continue;
                var flowInPortView = ((PassNodeView)nodeView).FlowInPortView;
                foreach (PassNodeData dependentNode in passNodeData.dependencies.SelectValue()) {
                    if (GetNodeView(dependentNode, out var dependentNodeView)) {
                        var dependenceNodeView = dependentNodeView as PassNodeView;
                        if (flowInPortView != null && dependenceNodeView != null) {
                            Edge edge = flowInPortView.ConnectTo(dependenceNodeView.FlowOutPortView);
                            // edge.layer = 1;
                            GraphView.FastAddElement(edge);
                        }
                    }
                }
            }
        }


        public IEnumerable<RPGNodeView> LoadNodeViews() {
            Loading = true;
            m_NodeViews.Clear();
            // Load Node from asset.
            foreach (var nodeData in currentGraph.NodeList) {
                RPGNodeView nodeView = nodeData switch {
                    PassNodeData passNodeData => new PassNodeView(passNodeData),
                    ResourceNodeData resourceNodeData => new ResourceNodeView(resourceNodeData),
                    _ => null
                };

                if (nodeView is null) // can not happen
                    continue;

                yield return nodeView;
                nodeView.Init();
                m_NodeViews[nodeData] = nodeView;
                nodeView.SetPos(nodeData.pos);
            }

            // Link Nodes
            foreach (PassNodeView passNodeView in currentGraph.NodeList.Select(nodeData => m_NodeViews[nodeData]).OfType<PassNodeView>()) {
                passNodeView.parameterViewModel.InitAttachEdge();
            }
            InitDependenceEdge();

            Loading = false;
        }

        public void UpdateNodeDependence(PassNodeView nodeViewView) {
            if (Loading) return;
            if (nodeViewView.Model is PassNodeData passNodeData) {
                var dependenceNodeDatas = nodeViewView.FlowInPortView.connections
                    .Select(t => t.output.node)
                    .Cast<PassNodeView>().Select(t => t.Model)
                    .Cast<PassNodeData>().ToList();
                bool needRecompile = false;
                if (dependenceNodeDatas.Count != passNodeData.dependencies.Count) {
                    needRecompile = true;
                }
                else {
                    for (int i = 0; i < dependenceNodeDatas.Count; ++i) {
                        if (dependenceNodeDatas[i] != passNodeData.dependencies[i]) {
                            needRecompile = true;
                        }
                    }
                }
                if (needRecompile) {
                    passNodeData.dependencies.Clear();
                    passNodeData.dependencies.AddRange(dependenceNodeDatas);
                }

            }
        }
        public ResourceNodeView CreateResourceNode(RPGBlackboardRow row) {
            var resourceNodeData = new ResourceNodeData() {
                Resource = row.model
            };
            currentGraph.m_NodeList.Add(resourceNodeData);
            var resourceNodeView = new ResourceNodeView(resourceNodeData);
            resourceNodeView.Init();
            m_NodeViews[resourceNodeData] = resourceNodeView;
            return resourceNodeView;
        }
    }
    public static class NodeViewExtension {
        public static void NotifyPositionChangeVM(this RPGNodeView nodeView, Vector2 vector2) {
            if (!NodeViewModel.GetValidViewModel(nodeView, out var nodeViewModel))
                return;
            nodeView.Model.pos = vector2;
            // nodeViewModel.Asset.NeedRecompile = true;
        }
        public static void NotifyDeleteVM(this RPGNodeView nodeView) {
            if (!NodeViewModel.GetValidViewModel(nodeView, out var nodeViewModel))
                return;
            nodeViewModel.currentGraph.m_NodeList.Remove(nodeView.Model);
            // nodeViewModel.Asset.NeedRecompile = true;
        }

    }
    public static class ResourceNodeViewExtension {
        public static void NotifyDisconnectAllPortVM(this ResourceNodeView resourceNodeView) {
            if (!NodeViewModel.GetValidViewModel(resourceNodeView, out var nodeViewModel))
                return;
            ResourcePortData port = resourceNodeView.Model.m_AttachTo;
            foreach (PortData linkTo in port.LinkTo) {
                PortData.Disconnect(port, linkTo);
            }
            // nodeViewModel.Asset.NeedRecompile = true;
        }

    }
}
