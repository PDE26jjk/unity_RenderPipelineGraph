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
                portDirection == Direction.Input ? Capacity.Single : Capacity.Multi, portType) {
            VisualElement connector = Children().First(t => t.name == "connector");
            connector.pickingMode = PickingMode.Position;

        }
        internal string ConnectorText {
            get => m_ConnectorText.text;
            set => m_ConnectorText.text = value;
        }
        public new RPGNodeEdge ConnectTo(RPGPortView other) => this.ConnectTo<RPGNodeEdge>(other);
        public override void DisconnectAll() {
            // resource --> parameter
            if (this.direction == Direction.Input) {
                GetFirstAncestorOfType<RPGParameterView>().NotifyDisconnectPort();
            }
            else {
                GetFirstAncestorOfType<ResourceNodeView>().NotifyDisconnectAllPort();
            }
            foreach (Edge connection in connections) {
                var otherPort = connection.input == this ? connection.output : connection.input;
                otherPort.Disconnect(connection);
                connection.RemoveFromHierarchy();
            }
            base.DisconnectAll();
        }
        public override void OnDrop(GraphView graphView, Edge edge) {
            base.OnDrop(graphView, edge);
            // GetFirstAncestorOfType<PassNode>()?.NotifyDependenceChange();
        }
    }
}
