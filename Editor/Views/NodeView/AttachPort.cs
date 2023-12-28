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


    public class AttachPortView : RPGPortView {

        public AttachPortView(Direction portDirection, Type portType) :
            base(Orientation.Horizontal, portDirection,
                portDirection == Direction.Input ? Capacity.Single
                    : Capacity.Multi, portType) {
            VisualElement connector = Children().First(t => t.name == "connector");
            connector.pickingMode = PickingMode.Position;
        }
        internal string ConnectorText {
            get => m_ConnectorText.text;
            set => m_ConnectorText.text = value;
        }

        public override void OnDrop(GraphView graphView, Edge edge) {
            base.OnDrop(graphView, edge);
            // GetFirstAncestorOfType<PassNode>()?.NotifyDependenceChange();
        }
    }
}
