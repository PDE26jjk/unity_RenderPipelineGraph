using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Editor.Views.Inspector;
using RenderPipelineGraph.Interface;
using RenderPipelineGraph.Serialization;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
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
        internal RPGBlackboard Blackboard => m_Blackboard;
        public override Blackboard GetBlackboard() => m_Blackboard;
        internal readonly NodeViewModel m_NodeViewModel;
        internal readonly ResourceViewModel m_ResourceViewModel;
        RPGAsset m_Asset;
        public RPGAsset Asset => m_Asset;
        class GraphUndoable : ScriptableObject, ISerializationCallbackReceiver {
            [NonReorderable]
            public RPGGraphData graph;

            [NonReorderable]
            public RPGView view;
            [NonReorderable]
            public List<string> graphContent = new(kMax);
            internal int version = 0;
            internal int lastVersion = 0;
            const int kMax = 100;
            int index = 0;
            int maxUndoIndex = 0;
            internal void Init() {
                graphContent.Add(MultiJson.Serialize(graph));
                for (int i = 1; i < kMax; i++) {
                    graphContent.Add(String.Empty);
                }
            }
            public void RecordUndo(string undoname) {
                version++;
                Debug.Log("RecordUndo " + undoname + version);
                // if (Selection.objects.Length > 0) {
                //     Undo.RecordObjects(Selection.objects, undoname);
                //     OnBeforeSerialize();
                // }
                // else {
                Undo.RecordObject(this, undoname);
                // }
            }
            public void OnBeforeSerialize() {
                if (lastVersion != version) {
                    lastVersion = version;
                    if (index == kMax - 1) {
                        graphContent.RemoveAt(0);
                    }
                    index++;
                    graphContent[index] = MultiJson.Serialize(graph);
                    maxUndoIndex = index;
                }
            }
            public void OnAfterDeserialize() {
            }
            public bool UndoGraphChange() {
                if (index >= 1) {
                    MultiJson.Deserialize(graph, graphContent[--index]);
                    return true;
                }
                return false;
            }
            public bool RedoGraphChange() {
                if (index < maxUndoIndex) {
                    index++;
                    MultiJson.Deserialize(graph, graphContent[index]);
                    return true;
                }
                return false;
            }
        }
        void OnEnterPanel(AttachToPanelEvent e) {
            Undo.undoRedoEvent += OnUndoEvent;
            Undo.undoRedoPerformed += undoRedoPerformed;
        }

        void OnLeavePanel(DetachFromPanelEvent e) {
            Undo.undoRedoEvent -= OnUndoEvent;
            Undo.undoRedoPerformed -= undoRedoPerformed;
        }
        List<ISelectable> tempSelectables;
        void undoRedoPerformed() {
            tempSelectables = selection.ToList();
        }
        void OnUndoEvent(in UndoRedoInfo undo) {
            if (!undo.isRedo) {
                if (m_GraphUndoable.UndoGraphChange()) {
                    ReloadModel();
                }
            }
            else {
                if (m_GraphUndoable.RedoGraphChange()) {
                    ReloadModel();
                }
            }
            AddRangeToSelection(tempSelectables);
        }

        internal RPGGraphData currentGraph;
        GraphUndoable m_GraphUndoable;
        public RPGView(RPGAsset asset) {
            this.m_Asset = asset;
            // asset.Graph.TestInit3();
            // asset.m_Graph = asset.Save();
            currentGraph = new RPGGraphData();
            MultiJson.Deserialize(currentGraph, asset.Content);
            m_GraphUndoable = ScriptableObject.CreateInstance<GraphUndoable>();
            m_GraphUndoable.graph = currentGraph;
            m_GraphUndoable.Init();

            m_NodeViewModel = new(this);
            m_ResourceViewModel = new ResourceViewModel(this);
            m_Blackboard = new RPGBlackboard(this, m_ResourceViewModel);
            m_Blackboard.layer = -1;
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
            AddLayer(0);
            AddLayer(1);
            AddLayer(2);
            focusable = true;

            m_Toolbar = new UnityEditor.UIElements.Toolbar();
            var b1 = new Button {
                text = "debug"
            };
            b1.clicked += () => {
                // Asset.debug1();
                ClearLayer(0);
                DeleteElements(edges);
                ClearLayer(1);
            };
            m_Toolbar.Add(b1);
            var objectField = new ObjectField {
                objectType = typeof(RPGAsset)
            };
            m_Toolbar.Add(objectField);
            Add(m_Toolbar);

            deleteSelection = Delete;
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            RegisterCallback<ValidateCommandEvent>(ValidateCommand);
            RegisterCallback<ExecuteCommandEvent>(ExecuteCommand);
            RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
            RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
            RegisterCallback<KeyDownEvent>(OnKeyDownEvent);
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

            ReloadModel();
        }
        void ReloadModel() {
            currentGraph = m_GraphUndoable.graph;
            m_NodeViewModel.Loading = true;
            m_Blackboard.ReloadModel();
            ClearLayer(0);
            DeleteElements(edges);
            ClearLayer(1);
            foreach (RPGNodeView loadNode in m_NodeViewModel.LoadNodeViews()) {
                FastAddElement(loadNode);
            }
            m_NodeViewModel.Loading = false;
        }
        void Delete(string cmd, AskUser askuser) {
            var currentSelection = selection.ToList();
            Debug.Log(cmd + currentSelection);
            // selection.RemoveRange();
            var nodeViews = selection.OfType<RPGNodeView>().ToList();
            foreach (RPGNodeView rpgNodeView in nodeViews) {
                rpgNodeView.Delete();
            }
            DeleteSelection();
            m_GraphUndoable.RecordUndo("delete");
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
                .OfType<RPGNodeView>().Select(t => t.Model)
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
            // Debug.Log("OnFocus");
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
            // Debug.Log("OnKeyDownEvent");
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
            // Debug.Log("ValidateCommand " + evt.commandName);
            // if (evt.commandName != "SoftDelete") {
            //     m_GraphUndoable.RecordUndo(evt.commandName);
            // }
        }

        void OnDragPerform(DragPerformEvent evt) {
            Debug.Log("OnDragPerform");
            if (getSelectingBlackboardField(out var rows)) {
                if (rows.Length > 0) {
                    DragAndDrop.AcceptDrag();
                    Vector2 mousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
                    float cpt = 0;
                    foreach (var row in rows) {
                        ResourceNodeView node = m_NodeViewModel.CreateResourceNode(row);
                        FastAddElement(node);
                        node.SetPos(mousePosition - new Vector2(50, 20) + cpt * new Vector2(0, 40));
                        ++cpt;
                    }
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

        internal void ClearLayer(int index) {
            var layer = (s_Method_GetLayer.Invoke(this, new object[] {
                index
            }) as VisualElement);
            layer?.Clear();
        }


        public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter) {
            List<Port> list = new();
            if (startAnchor is not RPGPortView port) {
                return list;
            }
            (port.node as RPGNodeView)?.NotifyPortDraggingStart(port);
            foreach (var n in this.nodes.ToList()) {
                if (n is RPGNodeView node) {
                    node.GetCompatiblePorts(list, port);
                }
            }
            return list;
        }
        public void NotifySelectionPositionChange() {
            foreach (IRPGMovable movable in selection.OfType<IRPGMovable>()) {
                movable.OnMoved();
            }
            m_GraphUndoable.RecordUndo("change position");
        }
    }
}
