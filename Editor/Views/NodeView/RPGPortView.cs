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


    public class RPGPortView : Port, IEdgeConnectorListener {

        public enum RPGPortType {
            Attach,
            Dependence
        }
        public static RPGPortView NewPort(RPGPortType rpgPortType,Direction direction, Type portType = null) {
            RPGPortView portView = rpgPortType switch {
                RPGPortType.Attach => new AttachPortView(direction,portType),
                RPGPortType.Dependence => new DependencePortView(direction),
                _ => throw new ArgumentOutOfRangeException(nameof(rpgPortType), rpgPortType, null)
            };
            return portView;
        }

        protected RPGPortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection,
            portCapacity,
            type) {
            m_EdgeConnector = new RPGNodeEdgeConnector(this);
            this.AddManipulator(m_EdgeConnector);
            this.m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
        }
        public virtual void OnDropOutsidePort(Edge edge, Vector2 position) {
            // Debug.Log("OnDropOutsidePort");
        }
        private GraphViewChange m_GraphViewChange;
        private List<Edge> m_EdgesToCreate = new();
        private List<GraphElement> m_EdgesToDelete = new();
        public virtual void OnDrop(GraphView graphView, Edge edge) {
            edge.layer = 1;
            // copy from decompiled Port.DefaultEdgeConnectorListener
            this.m_EdgesToCreate.Clear();
            this.m_EdgesToCreate.Add(edge);
            this.m_EdgesToDelete.Clear();
            if (edge.input.capacity == Port.Capacity.Single) {
                foreach (Edge connection in edge.input.connections) {
                    if (connection != edge)
                        this.m_EdgesToDelete.Add((GraphElement)connection);
                }
            }
            if (edge.output.capacity == Port.Capacity.Single) {
                foreach (Edge connection in edge.output.connections) {
                    if (connection != edge)
                        this.m_EdgesToDelete.Add((GraphElement)connection);
                }
            }
            if (this.m_EdgesToDelete.Count > 0)
                graphView.DeleteElements((IEnumerable<GraphElement>)this.m_EdgesToDelete);
            List<Edge> edgesToCreate = this.m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
                edgesToCreate = graphView.graphViewChanged(this.m_GraphViewChange).edgesToCreate;
            foreach (Edge edge1 in edgesToCreate) {
                graphView.AddElement((GraphElement)edge1);
                edge.input.Connect(edge1);
                edge.output.Connect(edge1);
            }
            // copy end

            // Debug.Log("OnDrop " + edge.input + edge.output);
            // Debug.Log(this.connections.Count());

        }
    }
}
