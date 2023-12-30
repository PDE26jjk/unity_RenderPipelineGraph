using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RenderPipelineGraph.Interface;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {

    public abstract class RPGNodeView : Node, IRPGMovable {

        protected NodeData m_Model;
        Rect lastPos = Rect.zero;

        // for show in inspector
        public NodeData Model {
            get => m_Model;
            set => m_Model = value;
        }
        protected RPGNodeView(NodeData model, string uxml = "UXML/GraphView/Node.uxml") : base(uxml) {
            this.layer = 0;
            base.UseDefaultStyling();
            m_Model = model;
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            // this.style.bottom
        }
        void IRPGMovable.OnMoved() {
            Rect newPos = GetPosition();
            if (lastPos != newPos) {
                // Debug.Log("moved");
                this.NotifyPositionChangeVM(new Vector2(newPos.x, newPos.y));
            }
            lastPos = newPos;
        }

        public void SetPos(Vector2 pos) {
            var newPos = new Rect(pos.x, pos.y, 0, 0);
            SetPosition(newPos);
            this.NotifyPositionChangeVM(pos);
            lastPos = GetPosition();
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
            Rect newPos = GetPosition();
            if (lastPos.x != 0 && lastPos.y != 0 && lastPos != newPos) {
                // this.NotifyPositionChangeVM();
                GetFirstAncestorOfType<RPGView>().NotifySelectionPositionChange();
            }
            // e.StopPropagation();
        }

        void OnPointerLeave(PointerLeaveEvent e) {
            // Debug.Log("OnPointerLeave");
            lastPos = GetPosition();
            // e.StopPropagation();
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
        public virtual void Delete() {
        }
    }
}
