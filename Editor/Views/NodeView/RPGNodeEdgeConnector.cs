using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {


    public class RPGNodeEdgeConnector : EdgeConnector<RPGNodeEdge> {
        RPGPortView m_PortView;
        public RPGNodeEdgeConnector(RPGPortView portView) : base(portView) {
            m_PortView = portView;
        }

        protected override void RegisterCallbacksOnTarget() {
            base.RegisterCallbacksOnTarget();
        }

    }
}