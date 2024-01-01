using System;
using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Serialization;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class NodeViewModel {
        public static bool GetValidViewModel(VisualElement visualElement, out NodeViewModel nodeViewModel) {
            var graphView = visualElement.GetFirstAncestorOfType<RPGView>();
            nodeViewModel = graphView?.m_NodeViewModel;
            return !(nodeViewModel is null || nodeViewModel.Loading || Undo.isProcessing);
        }
        internal Dictionary<NodeData, RPGNodeView> m_NodeViews = new();
        RPGView m_GraphView;
        public RPGView GraphView => m_GraphView;
        public RPGGraphData currentGraph => m_GraphView.currentGraph;
        public NodeViewModel(RPGView graphView) {
            this.m_GraphView = graphView;
        }
        public bool GetNodeView(NodeData nodeData, out RPGNodeView nodeView) {
            return m_NodeViews.TryGetValue(nodeData, out nodeView);
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
            Dictionary<NodeData, RPGNodeView> newNodeViews = new();
            // Load Node from asset.
            foreach (var nodeData in currentGraph.NodeList) {
                if (!GetNodeView(nodeData, out var nodeView)) {
                    nodeView = nodeData switch {
                        PassNodeData passNodeData => new PassNodeView(passNodeData),
                        ResourceNodeData resourceNodeData => new ResourceNodeView(resourceNodeData),
                        _ => null
                    };
                }
                if (nodeView is null) // can not happen
                    continue;

                yield return nodeView;
                nodeView.Init();
                nodeView.SetPos(nodeData.pos);
                newNodeViews[nodeData] = nodeView;
            }
            m_NodeViews.Clear();
            m_NodeViews = newNodeViews;

            // Link Nodes
            foreach (PassNodeView passNodeView in currentGraph.NodeList.Select(nodeData => m_NodeViews[nodeData]).OfType<PassNodeView>()) {
                passNodeView.parameterViewModel.InitAttachEdge();
            }
            InitDependenceEdge();
        }

        public ResourceNodeView CreateResourceNode(RPGBlackboardRow row) {
            var resourceNodeData = new ResourceNodeData() {
                Resource = row.Model
            };
            currentGraph.m_NodeList.Add(resourceNodeData);
            var resourceNodeView = new ResourceNodeView(resourceNodeData);
            m_GraphView.FastAddElement(resourceNodeView);
            resourceNodeView.Init();
            m_NodeViews[resourceNodeData] = resourceNodeView;
            return resourceNodeView;
        }
        public PassNodeView CreatePassNode(Type type) {
            if (!type.IsSubclassOf(typeof(RPGPass))) {
                throw new Exception($"{type.FullName} is not subclass of {typeof(RPGPass).FullName}.");
            }
            var passNodeData = PassNodeData.Instance(type);
            currentGraph.m_NodeList.Add(passNodeData);
            var passNodeView = new PassNodeView(passNodeData);
            m_GraphView.FastAddElement(passNodeView);
            passNodeView.Init();
            m_NodeViews[passNodeData] = passNodeView;
            foreach (RPGParameterView parameterView in passNodeView.ParameterViews) {
                parameterView.AfterInitEdge();
            }
            return passNodeView;
        }
    }
    public static class NodeViewExtension {
        public static void NotifyPositionChangeVM(this RPGNodeView nodeView, Vector2 moveDelta) {
            if (!NodeViewModel.GetValidViewModel(nodeView, out var nodeViewModel))
                return;
            nodeView.Model.pos += moveDelta;
            // nodeViewModel.Asset.NeedRecompile = true;
        }
        public static void NotifyDeleteVM(this RPGNodeView nodeView) {
            if (!NodeViewModel.GetValidViewModel(nodeView, out var nodeViewModel))
                return;
            nodeViewModel.currentGraph.m_NodeList.Remove(nodeView.Model);
            nodeViewModel.m_NodeViews.Remove(nodeView.Model);
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
        public static void NotifyDisconnectPortVM(this ResourceNodeView resourceNodeView, AttachPortView portView) {
            if (!NodeViewModel.GetValidViewModel(resourceNodeView, out var nodeViewModel))
                return;
            var parameterView = portView.GetFirstAncestorOfType<RPGParameterView>();
            if (parameterView is null) return;

            PortData.Disconnect(resourceNodeView.Model.m_AttachTo, parameterView.Model.Port);
        }
    }
    public static class PassNodeViewExtension {
        // should call after edge deleted.
        public static void NotifyDependenceChangeVM(this PassNodeView nodeViewView) {
            if (!NodeViewModel.GetValidViewModel(nodeViewView, out var nodeViewModel)) return;
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
    }
}
