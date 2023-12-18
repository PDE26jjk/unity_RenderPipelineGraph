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


    public class DependencePort : RPGPort {

        public DependencePort(Direction portDirection) : base(Orientation.Vertical, portDirection, Capacity.Multi,
            typeof(PassNode)) {
            // this.Remove(m_ConnectorText);
            this.tooltip = portDirection switch {
                Direction.Input => "dependence",
                Direction.Output => "dependBy",
                _ => null
            };
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        }
        void OnPointerEnter(PointerEnterEvent evt) {
            // evt.StopPropagation();
        }

        public override void OnDrop(GraphView graphView, Edge edge) {
            base.OnDrop(graphView, edge);
            GetFirstAncestorOfType<PassNode>()?.NotifyDependenceChange();
        }
        public override void Connect(Edge edge) {
            base.Connect(edge);
            GetFirstAncestorOfType<PassNode>()?.NotifyDependenceChange();
        }
        public override void Disconnect(Edge edge) {
            base.Disconnect(edge);
            GetFirstAncestorOfType<PassNode>()?.NotifyDependenceChange();
        }
        public override void DisconnectAll() {
            base.DisconnectAll();
            Debug.Log("-------------------disconnectAll");
            GetFirstAncestorOfType<PassNode>()?.NotifyDependenceChange();
        }

        internal Rect rect {
            get {
                Rect layout = this.layout;
                return new Rect(0.0f, 0.0f, layout.width, layout.height);
            }
        }
        
        // fix hover position check
        public override bool ContainsPoint(Vector2 localPoint) {
            Rect layout = this.m_ConnectorBox.layout;
            Rect rect1 = default;

            ref Rect local = ref rect1;
            double y = -(double)layout.yMin;
            Rect rect3 = this.rect;
            double width = (double)rect3.width - (double)layout.xMin;
            rect3 = this.rect;
            double height = (double)rect3.height;
            local = new Rect(0.0f, (float)y, (float)width, (float)height);
            double xMin = (double)layout.xMin;
            rect3 = this.m_ConnectorText.layout;
            double xMax = (double)rect3.xMax;
            float num = (float)(xMin - xMax);
            rect1.xMin -= num;
            rect1.width += num;
            return rect1.Contains(this.ChangeCoordinatesTo(this.m_ConnectorBox, localPoint));
        }
    }
}
