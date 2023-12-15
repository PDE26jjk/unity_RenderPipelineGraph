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
        public void InitEdge() {
            RPGView graphView = m_NodeView.GetFirstAncestorOfType<RPGView>();
            NodeViewModel nodeViewModel = graphView.m_ViewModel;
            foreach ((PortData portData, RPGPort portView) in m_PortViews) {
                foreach (PortData linkToPort in portData.linkTo) {
                    if (nodeViewModel.GetNodeView(linkToPort.owner, out var linkToNodeView)) {
                        RPGPort linkToPortView = linkToNodeView.m_PortViewModel.m_PortViews[linkToPort];
                        var output = portView.direction == Direction.Output ? portView : linkToPortView;
                        var input = portView.direction == Direction.Input ? portView : linkToPortView;
                        bool hasLinked = false;
                        foreach (Edge portViewConnection in portView.connections) {
                            if (portViewConnection.input == input && portViewConnection.output == output) {
                                hasLinked = true;
                                break;
                            }
                        }
                        if (!hasLinked) {
                            Edge edge = portView.ConnectTo(linkToPortView);
                            graphView.Add(edge);
                        }
                    }
                }
            }
        }
        public IEnumerable<RPGPort> LoadPortViews(RPGPort.DirectionType directionType = RPGPort.DirectionType.Output) {
            IEnumerable<PortData> portDatas = null;
            if (m_NodeView.model is PassNodeData passNodeData)
                switch (directionType) {
                    case RPGPort.DirectionType.Input:
                        portDatas = passNodeData.inputs.Values;
                        break;
                    case RPGPort.DirectionType.Output:
                        portDatas = passNodeData.outputs.Values;
                        break;
                    case RPGPort.DirectionType.Dependence:
                        portDatas = passNodeData.dependencies;
                        break;
                }
            else if (m_NodeView.model is ResourceNodeData resourceNodeData) {
                if (directionType == RPGPort.DirectionType.Output) {
                    portDatas = new PortData[] {
                        resourceNodeData.attachTo
                    };
                }
            }
            if (portDatas == null) {
                yield break;
            }
            foreach (PortData portData in portDatas) {
                RPGPort portView = null;
                switch (portData) {
                    case ResourcePortData resourcePortData:
                        portView = resourcePortData.resourceType switch {
                            ResourceType.Texture => RPGPort.NewPort(directionType, typeof(TextureHandle)),
                            ResourceType.Buffer => RPGPort.NewPort(directionType, typeof(BufferHandle)),
                            ResourceType.AccelerationStructure => RPGPort.NewPort(directionType, typeof(RayTracingAccelerationStructureHandle)),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        break;
                    case DependencePortData dependencePortData:
                        portView = RPGPort.DependencePort();
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
