using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class PortViewModel {
        readonly Dictionary<PortData, RPGPort> m_PortViews = new();
        readonly RPGNode m_NodeView;
        public RPGNode NodeView =>
            m_NodeView;
        public PortViewModel(RPGNode nodeView) {
            m_NodeView = nodeView;
            Reset();
        }

        internal void Reset() {
            m_PortViews.Clear();
        }
        // public static RPGNode GetPortView(RPGView graphView,PortData portData) {
        //     NodeViewModel nodeViewModel = graphView.m_ViewModel;
        //     
        // }
        public void InitDependenceEdge() {
            if (m_NodeView.Model is not PassNodeData passNodeData) return;
            NodeViewModel nodeViewModel = GetNodeViewModel();
            DependencePort flowInPort = (m_NodeView as PassNode)?.FlowInPort;
            foreach (PassNodeData nodeData in passNodeData.dependencies) {
                if (nodeViewModel.GetNodeView(nodeData, out var nodeView)) {
                    var dependenceNodeView = nodeView as PassNode;
                    if (flowInPort != null && dependenceNodeView != null) {
                        Edge edge = flowInPort.ConnectTo(dependenceNodeView.FlowOutPort);
                        nodeViewModel.GraphView.Add(edge);
                    }
                }
            }
        }
        
        public void InitAttachEdge() {
            NodeViewModel nodeViewModel = GetNodeViewModel();
            foreach ((PortData portData, RPGPort portView) in m_PortViews) {
                foreach (PortData linkToPort in portData.linkTo) {
                    if (nodeViewModel.GetNodeView(linkToPort.owner, out var linkToNodeView)) {
                        RPGPort linkToPortView = linkToNodeView.m_PortViewModel.m_PortViews[linkToPort];
                        var output = portView.direction == Direction.Output ? portView : linkToPortView;
                        var input = portView.direction == Direction.Input ? portView : linkToPortView;
                        bool hasLinked = false;
                        // link once
                        foreach (Edge portViewConnection in portView.connections) {
                            if (portViewConnection.input == input && portViewConnection.output == output) {
                                hasLinked = true;
                                break;
                            }
                        }
                        if (!hasLinked) {
                            Edge edge = portView.ConnectTo(linkToPortView);
                            nodeViewModel.GraphView.Add(edge);
                        }
                    }
                }
            }
        }
        
        NodeViewModel GetNodeViewModel() {
            RPGView graphView = m_NodeView.GetFirstAncestorOfType<RPGView>();
            NodeViewModel nodeViewModel = graphView.m_NodeViewModel;
            return nodeViewModel;
        }

        public IEnumerable<RPGPort> LoadAttachPortViews() {
            var rpgPortType = RPGPort.RPGPortType.Attach;
            IEnumerable<PortData> portDatas = m_NodeView.Model switch {
                PassNodeData passNodeData => passNodeData.Attachments.Values,
                ResourceNodeData resourceNodeData => new[] {
                    resourceNodeData.attachTo
                },
                _ => throw new ArgumentOutOfRangeException()
            };
            Direction direction = m_NodeView.Model switch {
                PassNodeData passNodeData => Direction.Input,
                ResourceNodeData resourceNodeData => Direction.Output,
                _ => throw new ArgumentOutOfRangeException()

            };
            foreach (PortData portData in portDatas) {
                RPGPort portView = null;
                switch (portData) {
                    case ResourcePortData resourcePortData:
                        portView = resourcePortData.resourceType switch {
                            ResourceType.Texture => RPGPort.NewPort(rpgPortType, direction, typeof(TextureHandle)),
                            ResourceType.Buffer => RPGPort.NewPort(rpgPortType, direction, typeof(BufferHandle)),
                            ResourceType.AccelerationStructure => RPGPort.NewPort(rpgPortType, direction, typeof(RayTracingAccelerationStructureHandle)),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        break;
                    case DependencePortData dependencePortData:
                        // portView = RPGPort.DependencePort();
                        break;
                }
                if (portView is not null) {
                    m_PortViews[portData] = portView;
                    portView.portName = portData.name;
                    yield return portView;
                }
                else {
                    throw new Exception("LoadPortViews");
                }
            }
        }
    }
}
