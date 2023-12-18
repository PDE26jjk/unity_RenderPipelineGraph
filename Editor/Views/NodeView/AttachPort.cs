using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {


    public class AttachPort : RPGPort {

        public AttachPort(Direction portDirection, Type portType) :
            base(Orientation.Horizontal, portDirection,
            portDirection == Direction.Input ? Capacity.Multi : Capacity.Single, portType) {
        }

        public override void OnDrop(GraphView graphView, Edge edge) {
            base.OnDrop(graphView, edge);
            // GetFirstAncestorOfType<PassNode>()?.NotifyDependenceChange();
        }
    }
}
