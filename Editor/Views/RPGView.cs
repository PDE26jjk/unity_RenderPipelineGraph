using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Editor.Views.Inspector;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class RPGView : GraphView, IDisposable {

        SelectionDragger m_SelectionDragger;
        RectangleSelector m_RectangleSelector;
        // internal ICollection<testGraphPort> getPorts() {
        //     return Ports.AsReadOnlyCollection();
        // }
        InspectorView m_Inspector;
        private Toolbar m_Toolbar;
        readonly RPGBlackboard m_Blackboard;
        readonly internal NodeViewModel m_NodeViewModel;
        public RPGAsset Asset => m_NodeViewModel.Asset;
        public RPGView(RPGAsset asset) {
            // asset.Graph.TestInit3();
            asset.m_Graph = asset.Save();
            m_NodeViewModel = new(this, asset);
            m_Blackboard = new RPGBlackboard(this);
            SetupZoom(0.125f, 8);

            // bool blackboardVisible = BoardPreferenceHelper.IsVisible(BoardPreferenceHelper.Board.blackboard, true);
            // if (blackboardVisible)
            Add(m_Blackboard);

            // Add(m_Inspector);

            this.AddManipulator(new ContentDragger());
            // 选择
            m_SelectionDragger = new SelectionDragger();
            this.AddManipulator(m_SelectionDragger);
            // 框选
            m_RectangleSelector = new RectangleSelector();
            this.AddManipulator(m_RectangleSelector);
            this.AddManipulator(new FreehandSelector());

            AddLayer(-1);
            AddLayer(1);
            AddLayer(2);
            focusable = true;

            m_Toolbar = new UnityEditor.UIElements.Toolbar();
            var b1 = new Button {
                text = "debug"
            };
            b1.clicked += () => { Asset.debug1(); };
            m_Toolbar.Add(b1);
            var objectField = new ObjectField {
                objectType = typeof(RPGAsset)
            };
            m_Toolbar.Add(objectField);
            Add(m_Toolbar);

            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            RegisterCallback<ValidateCommandEvent>(ValidateCommand);
            RegisterCallback<ExecuteCommandEvent>(ExecuteCommand);
            RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
            RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
            RegisterCallback<KeyDownEvent>(OnKeyDownEvent);
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

            foreach (RPGNode loadNode in m_NodeViewModel.LoadNodeViews(asset)) {
                FastAddElement(loadNode);
            }
        }
        public void Dispose() {
            UnregisterCallback<DragUpdatedEvent>(OnDragUpdated);
            UnregisterCallback<DragPerformEvent>(OnDragPerform);
            UnregisterCallback<ValidateCommandEvent>(ValidateCommand);
            UnregisterCallback<ExecuteCommandEvent>(ExecuteCommand);
            UnregisterCallback<AttachToPanelEvent>(OnEnterPanel);
            UnregisterCallback<DetachFromPanelEvent>(OnLeavePanel);
            UnregisterCallback<KeyDownEvent>(OnKeyDownEvent);
            UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            // EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        Vector2 m_PasteCenter;
        public void UpdateGlobalSelection() {
            var objectSelected = selection
                .OfType<RPGNode>().Select(t => t.Model)
                .Where(t => t != null)
                .ToArray();

            if (objectSelected.Length > 0) {
                Selection.objects = objectSelected.toInspectorBinding();
                return;
            }

            var blackBoardSelected = selection.OfType<RPGBlackboardField>().Select(t => t.GetFirstAncestorOfType<RPGBlackboardRow>()?.model).ToArray();

            if (blackBoardSelected.Length > 0) {
                Selection.objects = blackBoardSelected.toInspectorBinding();
                return;
            }
        }
        public void OnFocus() {
            Debug.Log("OnFocus");
            if (selection.Any(x => x.HitTest(m_PasteCenter))) {
                UpdateGlobalSelection();
            }
        }
        public void AddRangeToSelection(List<ISelectable> selectables) {
            selectables.ForEach(base.AddToSelection);
            UpdateGlobalSelection();
        }

        public override void AddToSelection(ISelectable selectable) {
            base.AddToSelection(selectable);
            UpdateGlobalSelection();
        }
        bool m_Dirty = false;
        public bool Dirty {
            get => m_Dirty;
            set => m_Dirty = value;
        }
        public void SetBoardToFront(GraphElement board) {
            board.SendToBack();
            board.PlaceBehind(m_Toolbar);
        }
        void OnMouseMoveEvent(MouseMoveEvent evt) {
            // Debug.Log("OnMouseMoveEvent");
            m_PasteCenter = evt.mousePosition;
        }
        void OnKeyDownEvent(KeyDownEvent evt) {
            Debug.Log("OnKeyDownEvent");
        }
        void OnLeavePanel(DetachFromPanelEvent evt) {
            Debug.Log("OnLeavePanel");
        }
        void OnEnterPanel(AttachToPanelEvent evt) {
            Debug.Log("OnEnterPanel");
        }
        void ExecuteCommand(ExecuteCommandEvent evt) {

            Debug.Log("ExecuteCommand " + evt.commandName);
            if (evt.commandName == "SelectAll") {
                ClearSelection();

                AddRangeToSelection(graphElements.OfType<ISelectable>().ToList());
                evt.StopPropagation();
            }
        }
        void ValidateCommand(ValidateCommandEvent evt) {
            Debug.Log("ValidateCommand" + evt.commandName);
        }

        void OnDragPerform(DragPerformEvent evt) {
            Debug.Log("OnDragPerform");
            if (getSelectingBlackboardField(out var rows)) {
                if (rows.Length > 0) {
                    DragAndDrop.AcceptDrag();
                    Vector2 mousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
                    float cpt = 0;
                    // foreach (var row in rows) {
                    //     ResourceNode node = new TextureNode();
                    //     node.SetPos(mousePosition - new Vector2(50, 20) + cpt * new Vector2(0, 40));
                    //     FastAddElement(node);
                    //     ++cpt;
                    // }
                }
            }
            evt.StopPropagation();
        }
        void OnDragUpdated(DragUpdatedEvent evt) {
            Debug.Log("OnDragUpdated...");
            if (getSelectingBlackboardField(out var rows)) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                evt.StopPropagation();
            }
        }
        bool getSelectingBlackboardField(out RPGBlackboardRow[] rows) {
            rows = selection.OfType<RPGBlackboardField>().Select(t => t.GetFirstAncestorOfType<RPGBlackboardRow>()).Where(t => t != null).ToArray();
            return DragAndDrop.GetGenericData("DragSelection") != null &&
                   selection.Any(t => t is RPGBlackboardField && (t as RPGBlackboardField).GetFirstAncestorOfType<RPGBlackboardRow>() != null);
        }
        static FieldInfo s_Member_ContainerLayer = typeof(GraphView).GetField("m_ContainerLayers", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo s_Method_GetLayer = typeof(GraphView).GetMethod("GetLayer", BindingFlags.NonPublic | BindingFlags.Instance);


        public void FastAddElement(GraphElement graphElement) {
            if (graphElement.IsResizable()) {
                graphElement.hierarchy.Add(new Resizer());
                graphElement.style.borderBottomWidth = 6;
            }

            int newLayer = graphElement.layer;
            if (!((IDictionary)s_Member_ContainerLayer.GetValue(this)).Contains(newLayer)) {
                AddLayer(newLayer);
            }
            (s_Method_GetLayer.Invoke(this, new object[] {
                newLayer
            }) as VisualElement)?.Add(graphElement);
        }


        public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter) {
            List<Port> list = new();
            if (startAnchor is not RPGPort port) {
                return list;
            }
            (port.node as RPGNode)?.NotifyPortDraggingStart(port);
            foreach (var n in this.nodes.ToList()) {
                if (n is RPGNode node) {
                    node.GetCompatiblePorts(list, port);
                }
            }
            return list;
        }
    }
}
