using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {

    public abstract class RPGNodeView : Node {

        protected NodeData m_Model;
        
        // for show in inspector
        public NodeData Model {
            get => m_Model;
            set => m_Model = value;
        }
        protected RPGNodeView(NodeData model, string uxml = "UXML/GraphView/Node.uxml") : base(uxml) {
            base.UseDefaultStyling();
            m_Model = model;
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<FocusInEvent>(OnFocusIn);
        }

        public void SetPos(Vector2 pos) {
            SetPosition(new Rect(pos.x, pos.y, 0, 0));
            this.NotifyPositionChange(pos);
        }
        
        public virtual void GetCompatiblePorts(List<Port> list, RPGPortView portViewToConnect) {
            if (portViewToConnect.node == this) return;
            switch (portViewToConnect.node) {
                case ResourceNodeView when this is PassNodeView:
                case PassNodeView when this is ResourceNodeView:
                    break;
                default:
                    return;
            }
            var container = portViewToConnect.direction == Direction.Input ? outputContainer : inputContainer;
            foreach (var p in container.Children()) {
                if (p is RPGPortView port && port.portType == portViewToConnect.portType)
                    list.Add(port);
            }
        }

        public virtual void NotifyPortDraggingStart(Port port) {
        }

        void OnPointerEnter(PointerEnterEvent e) {
            // Debug.Log("OnPointerEnter");
            e.StopPropagation();
        }

        void OnPointerLeave(PointerLeaveEvent e) {
            // Debug.Log("OnPointerLeave");
            e.StopPropagation();
        }
        void OnFocusIn(FocusInEvent e) {
            // Debug.Log("OnFocusIn");
            var gv = GetFirstAncestorOfType<RPGView>();
            if (!IsSelected(gv))
                Select(gv, false);
            e.StopPropagation();
        }
        public virtual void Init() {
        }
    }
}
