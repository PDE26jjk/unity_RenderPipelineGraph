using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class NodeViewModel {
        public static NodeViewModel GetViewModel(VisualElement visualElement) {
            RPGView graphView = visualElement.GetFirstAncestorOfType<RPGView>();
            return graphView?.m_NodeViewModel;
        }
        internal Dictionary<NodeData, RPGNodeView> m_NodeViews = new();
        RPGView m_GraphView;
        public RPGView GraphView => m_GraphView;
        public NodeViewModel(RPGView graphView, RPGAsset asset) {
            this.m_GraphView = graphView;
            this.m_Asset = asset;
        }
        public bool GetNodeView(NodeData nodeData, out RPGNodeView nodeViewView) {
            return m_NodeViews.TryGetValue(nodeData, out nodeViewView);
        }
        internal bool Loading = false;

        bool m_AssetDirty = false;
        readonly RPGAsset m_Asset;
        public RPGAsset Asset => m_Asset;
        public bool AssetDirty => m_AssetDirty;

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
                            GraphView.Add(edge);
                        }
                    }
                }
            }
        }


        public IEnumerable<RPGNodeView> LoadNodeViews(RPGAsset asset) {
            Loading = true;
            m_NodeViews.Clear();
            // Load Node from asset.
            foreach (var nodeData in asset.Graph.NodeList) {
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
            foreach (PassNodeView passNodeView in asset.Graph.NodeList.Select(nodeData => m_NodeViews[nodeData]).OfType<PassNodeView>()) {
                passNodeView.parameterViewModel.InitAttachEdge();
            }
            InitDependenceEdge();

            Loading = false;
        }

        public void UpdateNodeDependence(PassNodeView nodeViewView) {
            if (Loading) return;
            m_AssetDirty = true;
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
                    GraphView.Asset.NeedRecompile = true;
                }

            }
        }
        public ResourceNodeView CreateResourceNode(RPGBlackboardRow row) {
            var resourceNodeData = new ResourceNodeData() {
                Resource = row.model
            };
            Asset.Graph.m_NodeList.Add(resourceNodeData);
            Asset.NeedRecompile = true;
            var resourceNodeView = new ResourceNodeView(resourceNodeData);
            resourceNodeView.Init();
            m_NodeViews[resourceNodeData] = resourceNodeView;
            return resourceNodeView;
        }
    }
    public static class NodeViewExtension {
        public static void NotifyPositionChange(this RPGNodeView nodeView, Vector2 vector2) {
            var nodeViewModel = NodeViewModel.GetViewModel(nodeView);
            if (nodeViewModel is null || nodeViewModel.Loading) return;
            nodeViewModel.Asset.NeedRecompile = true;
            nodeView.Model.pos = vector2;
        }
    }
}
