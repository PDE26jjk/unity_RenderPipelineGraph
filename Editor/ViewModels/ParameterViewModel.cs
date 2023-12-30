using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class ParameterViewModel {
        internal NodeViewModel GetNodeViewModel() {
            RPGView graphView = m_NodeView.GetFirstAncestorOfType<RPGView>();
            NodeViewModel nodeViewModel = graphView.m_NodeViewModel;
            return nodeViewModel;
        }

        readonly Dictionary<RPGParameterData, RPGParameterView> m_ParamViews = new();
        internal bool TryGetParamView(RPGParameterData parameterData, out RPGParameterView parameterView) {
            return m_ParamViews.TryGetValue(parameterData, out parameterView);
        }
        readonly PassNodeView m_NodeView;
        public PassNodeView NodeView =>
            m_NodeView;
        public ParameterViewModel(PassNodeView nodeView) {
            m_NodeView = nodeView;
            Reset();
        }

        internal void Reset() {
            m_ParamViews.Clear();
        }
        // public static RPGNode GetPortView(RPGView graphView,PortData portData) {
        //     NodeViewModel nodeViewModel = graphView.m_ViewModel;
        //     
        // }


        public void InitAttachEdge() {
            NodeViewModel nodeViewModel = GetNodeViewModel();
            if (!nodeViewModel.Loading) return;
            foreach ((RPGParameterData parameterData, RPGParameterView parameterView) in m_ParamViews) {
                if (parameterData.Port.LinkTo.Count != 0) {
                    var portLinkTo = parameterData.Port.LinkTo.First() as ResourcePortData;
                    if (parameterData.NeedPort() && portLinkTo is not null) {
                        nodeViewModel.GetNodeView(portLinkTo.Owner as NodeData, out RPGNodeView linkToNodeView);
                        if (linkToNodeView is not ResourceNodeView resourceNodeView)
                            throw new Exception();
                        var edge = resourceNodeView.PortView.ConnectTo(parameterView.PortView);
                        nodeViewModel.GraphView.FastAddElement(edge);
                    }
                }
                parameterView.AfterInitEdge();
            }
        }


        public IEnumerable<RPGParameterView> LoadParameterViews() {

            foreach (RPGParameterData parameterData in this.m_NodeView.Model.Parameters.Values) {
                RPGParameterView parameterView = parameterData switch {
                    CullingResultParameterData cullingResultParameterData => new cullingResultParamView(this, cullingResultParameterData),
                    RendererListParameterData rendererListParameterData => new RendererListParamView(this, rendererListParameterData),
                    TextureListParameterData textureListParameterData => new TextureListParamView(this, textureListParameterData),
                    TextureParameterData textureParameterData => new TextureParamView(this, textureParameterData),
                    _ => throw new ArgumentOutOfRangeException(nameof(parameterData))
                };
                parameterView.Init();
                m_ParamViews[parameterData] = parameterView;
                yield return parameterView;
            }
        }
    }
    public static class ParameterViewExtension {
        public static void NotifyDisconnectPortVM(this RPGParameterView parameterView) {
            if (!NodeViewModel.GetValidViewModel(parameterView, out var nodeViewModel))
                return;
            ResourcePortData resourcePortData = parameterView.Model.Port;
            if (resourcePortData.LinkTo.Count > 0)
                PortData.Disconnect(resourcePortData, resourcePortData.LinkTo[0]);
            // nodeViewModel.Asset.NeedRecompile = true;
        }
    }
}
