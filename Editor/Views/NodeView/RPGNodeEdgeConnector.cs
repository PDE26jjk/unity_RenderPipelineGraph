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
        RPGPort m_port;
        public RPGNodeEdgeConnector(RPGPort port) : base(port) {
            m_port = port;
        }

        protected override void RegisterCallbacksOnTarget() {
            base.RegisterCallbacksOnTarget();
        }

    }
}