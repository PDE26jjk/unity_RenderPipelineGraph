using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class NodeViewModel {
        Dictionary<NodeData, RPGNode> m_NodeViews = new();
        RPGView m_GraphView;
        public RPGView GraphView => m_GraphView;
        public NodeViewModel(RPGView graphView) {
            this.m_GraphView = graphView;
        }
        public bool GetNodeView(NodeData nodeData , out RPGNode nodeView) {
            return m_NodeViews.TryGetValue(nodeData, out nodeView);
        }
        public IEnumerable<RPGNode> LoadNodes(RPGAsset asset) {
            m_NodeViews.Clear();
            // Load Node from asset.
            foreach (NodeData nodeData in asset.NodeList) {
                RPGNode nodeView = nodeData switch {
                    PassNodeData passNodeData => new PassNode(passNodeData),
                    ResourceNodeData resourceNodeData => new TextureNode(resourceNodeData),
                    _ => null
                };

                if (nodeView is not null) {
                    m_NodeViews[nodeData] = nodeView;
                    nodeView.title = nodeData.exposedName;
                    nodeView.SetPos(nodeData.pos);
                    yield return nodeView;
                }
            }
            // Link Nodes
            foreach (NodeData nodeData in asset.NodeList) {
                var nodeView = m_NodeViews[nodeData];
                nodeView.m_PortViewModel.InitEdge();
                // foreach (var kvp in nodeData.inputs) {
                //     var port = nodeView.inputContainer[0] as RPGPort;
                //     // port.ConnectTo()
                //     // nodeDataInput
                // }
            }
        }
    }
}
