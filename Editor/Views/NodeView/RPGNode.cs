using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.NodeView {


    public class RPGNode : Node {

        public RPGNode() {
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            Port port1 = RPGPort.inputPort();
            Port port2 = RPGPort.outputPort();
            inputContainer.Add(port1);
            outputContainer.Add(port2);
        }
        public void GetCompatiblePorts(List<Port> list, RPGPort portToConnect) {
            if (portToConnect.node == this) return;
            var container = portToConnect.direction == Direction.Input ? outputContainer : inputContainer;
            foreach (var p in container.Children()) {
                list.Add(p as RPGPort);
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