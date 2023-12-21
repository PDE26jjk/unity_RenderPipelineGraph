using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Serialization;

namespace RenderPipelineGraph {
    public class NodeViewModel {
        internal Dictionary<Slottable, RPGNode> m_NodeViews = new();
        RPGView m_GraphView;
        public RPGView GraphView => m_GraphView;
        public NodeViewModel(RPGView graphView, RPGAsset asset) {
            this.m_GraphView = graphView;
            this.m_Asset = asset;
        }
        public bool GetNodeView(Slottable nodeData, out RPGNode nodeView) {
            return m_NodeViews.TryGetValue(nodeData, out nodeView);
        }
        bool loading = false;

        bool m_AssetDirty = false;
        readonly RPGAsset m_Asset;
        public RPGAsset Asset => m_Asset;
        public bool AssetDirty => m_AssetDirty;
        public IEnumerable<RPGNode> LoadNodeViews(RPGAsset asset) {
            loading = true;
            m_NodeViews.Clear();
            // Load Node from asset.
            foreach (var nodeData in asset.Graph.NodeList) {
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
            foreach (NodeData nodeData in asset.Graph.NodeList) {
                var nodeView = m_NodeViews[nodeData];
                nodeView.m_PortViewModel.InitAttachEdge();
                if (nodeView is PassNode) nodeView.m_PortViewModel.InitDependenceEdge();
                // foreach (var kvp in nodeData.inputs) {
                //     var port = nodeView.inputContainer[0] as RPGPort;
                //     // port.ConnectTo()
                //     // nodeDataInput
                // }
            }
            loading = false;
        }

        public void UpdateNodeDependence(PassNode nodeView) {
            if (loading) return;
            m_AssetDirty = true;
            if (nodeView.Model is PassNodeData passNodeData) {
                passNodeData.dependencies.Clear();
                foreach (var dependenceNodeData in nodeView.FlowInPort.connections
                    .Select(t => t.output.node)
                    .Cast<PassNode>().Select(t => t.Model)
                    .Cast<PassNodeData>()) {
                    passNodeData.dependencies.Add(dependenceNodeData);
                }
            }
        }
    }
}
