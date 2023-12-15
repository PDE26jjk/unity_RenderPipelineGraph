using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {

    public abstract class RPGNode : Node {

        protected NodeData m_Model;
        readonly internal PortViewModel m_PortViewModel;
        
        public NodeData model {
            get => m_Model;
            set => m_Model = value;
        }
        public RPGNode(NodeData model) {
            m_Model = model;
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            m_PortViewModel = new PortViewModel(this);
            Init();
        }
        protected virtual void Init() {
            foreach (RPGPort portView in m_PortViewModel.LoadPortViews(RPGPort.DirectionType.Input)) {
                this.inputContainer.Add(portView);
            }
            foreach (RPGPort portView in m_PortViewModel.LoadPortViews(RPGPort.DirectionType.Output)) {
                this.outputContainer.Add(portView);
            }
        }
        public void SetPos(Vector2 pos) {
            SetPosition(new Rect(pos.x, pos.y, 0, 0));
        }
        public void GetCompatiblePorts(List<Port> list, RPGPort portToConnect) {
            if (portToConnect.node == this) return;
            switch (portToConnect.node) {
                case ResourceNode when this is PassNode:
                case PassNode when this is ResourceNode:
                    break;
                default:
                    return;
            }
            var container = portToConnect.direction == Direction.Input ? outputContainer : inputContainer;
            foreach (var p in container.Children()) {
                if (p is RPGPort port && port.portType == portToConnect.portType)
                    list.Add(port);
            }
        }

        void OnPointerEnter(PointerEnterEvent e) {
            Debug.Log("OnPointerEnter");
            e.StopPropagation();
        }

        void OnPointerLeave(PointerLeaveEvent e) {
            Debug.Log("OnPointerLeave");
            e.StopPropagation();
        }
        void OnFocusIn(FocusInEvent e) {
            Debug.Log("OnFocusIn");
            var gv = GetFirstAncestorOfType<RPGView>();
            if (!IsSelected(gv))
                Select(gv, false);
            e.StopPropagation();
        }
    }
}
