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
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
            [NonSerialized]
            public RPGGraphData graph;

            [NonSerialized]
            public RPGView view;
            [NonSerialized]
            public List<string> graphContent = new(kMax);
            [SerializeField]
            internal int version = 0;
            internal int lastVersion = 0;
            const int kMax = 100;
            int index = 0;
            int maxVersion = 0;
            internal void Init() {
                graphContent.Add(MultiJson.Serialize(graph));
                for (int i = 1; i < kMax; i++) {
                    graphContent.Add(String.Empty);
                }
                maxVersion = version = 1;
            }
            public void RecordUndo(string undoname) {
                Debug.Log("RecordUndo " + undoname + version);
                Undo.RecordObject(this, undoname);
                version++;
                maxVersion = version;
            }
            public void OnBeforeSerialize() {
                // if (Undo.isProcessing) return;
                if (lastVersion != version) {
                    lastVersion = version;
                    string json = MultiJson.Serialize(graph);
                    // Debug.Log(json);
                    if (lastVersion >= graphContent.Count) {
                        graphContent.Add(string.Empty);
                    }
                    graphContent[lastVersion] = json;
                    Debug.Log("OnBeforeSerialize " + lastVersion);
                }
            }
            public void OnAfterDeserialize() {
                // if (Undo.isProcessing)
                if (graph is not null) {
                    Debug.Log("OnAfterDeserialize " + version);
                    MultiJson.Deserialize(graph, graphContent[version]);
                }
            }
            public bool UndoGraphChange() {

                if (lastVersion != version) {
                    lastVersion = version;
                    return true;
                }
                return false;
            }
            public bool RedoGraphChange() {

                if (lastVersion != version) {
                    lastVersion = version;
                    return true;
                }
                return false;
            }
        }
        void OnEnterPanel(AttachToPanelEvent e) {
            Undo.undoRedoEvent += OnUndoEvent;
            Undo.undoRedoPerformed += undoRedoPerformed;
            Undo.postprocessModifications += postprocessModifications;
        }


        void OnLeavePanel(DetachFromPanelEvent e) {
            Undo.undoRedoEvent -= OnUndoEvent;
            Undo.undoRedoPerformed -= undoRedoPerformed;
            Undo.postprocessModifications -= postprocessModifications;
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

        UndoPropertyModification[] postprocessModifications(UndoPropertyModification[] modifications) {
            return modifications;
        }

        internal RPGGraphData currentGraph;
        GraphUndoable m_GraphUndoable;
        internal void RecordUndo(string undoname) {
            m_GraphUndoable.RecordUndo(undoname);
        }
        public RPGView(RPGAsset asset) {
            this.m_Asset = asset;
            // asset.Graph.TestInit3();
            // asset.m_Graph = asset.Save();
            // if (!asset.Deserialized) asset.Deserialize();
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
            var b0 = new Button {
                text = "debug"
            };
            b0.clicked += () => {
                Debug.Log(Asset.Content);
                string serialize = MultiJson.Serialize(Asset.m_Graph);
                Debug.Log(serialize);
                Assert.IsTrue(Asset.Content == serialize);
            };
            m_Toolbar.Add(b0);
            var b1 = new Button {
                text = "init"
            };
            b1.clicked += () => {
                // Asset.debug1();
                // ClearLayer(0);
                // DeleteElements(edges);
                // ClearLayer(1);
                RecordUndo("init");
                currentGraph.TestInit3();
                asset.Save(currentGraph);
                ReloadModel();
            };
            m_Toolbar.Add(b1);
            var b2 = new Button {
                text = "save"
            };
            b2.clicked += () => {
                // Asset.debug1();
                // Asset.m_Graph = currentGraph;
                Asset.Save(currentGraph);
            };
            m_Toolbar.Add(b2);
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
            graphViewChanged += OnGraphView;
            ReloadModel();
        }
        // handle user move nodes, delete things, change edges.
        GraphViewChange OnGraphView(GraphViewChange changeData) {
            if (Undo.isProcessing || m_NodeViewModel.Loading)
                return changeData;
            List<GraphElement> elementsToRemove = changeData.elementsToRemove;
            List<GraphElement> changeDataMovedElements = changeData.movedElements;
            if (changeDataMovedElements is not null && changeDataMovedElements.Count > 0) {
                RecordUndo("change position");
                foreach (IRPGMovable movable in changeDataMovedElements.OfType<IRPGMovable>()) {
                    movable.OnMoved(changeData.moveDelta);
                }
            }
            if (elementsToRemove is not null && elementsToRemove.Count > 0) {
                RecordUndo("delete");
                var deletables = selection.OfType<IRPGDeletable>().ToList();
                foreach (var deletable in deletables) {
                    deletable.OnDelete();
                }
            }
            return changeData;
        }
        void ReloadModel() {
            currentGraph = m_GraphUndoable.graph;
            m_NodeViewModel.Loading = true;
            m_Blackboard.ReloadModel();
            ClearLayer(0);
            ClearLayer(1);
            DeleteElements(edges);
            foreach (RPGNodeView loadNode in m_NodeViewModel.LoadNodeViews()) {
                FastAddElement(loadNode);
            }
            m_NodeViewModel.Loading = false;
        }
        void Delete(string cmd, AskUser askuser) {
            // var currentSelection = selection.ToList();
            // Debug.Log(cmd + currentSelection);
            // // selection.RemoveRange();
            // var deletables = selection.OfType<IRPGDeletable>().ToList();
            // foreach (var deletable in deletables) {
            //     deletable.OnDelete();
            // }

            DeleteSelection();
            // m_GraphUndoable.RecordUndo("delete");
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
            m_GraphUndoable.graph = null;
            graphViewChanged -= OnGraphView;
            // EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        Vector2 m_PasteCenter;
        public void UpdateGlobalSelection() {
            var bindables = selection
                .OfType<IRPGBindable>().Select(t => t.BindingObject()).Cast<Object>()
                .ToArray();

            if (bindables.Length > 0) {
                Selection.objects = bindables;
                return;
            }
            //
            // var blackBoardSelected = selection.OfType<RPGBlackboardField>().Select(t => t.GetFirstAncestorOfType<RPGBlackboardRow>()?.Model).ToArray();
            //
            // if (blackBoardSelected.Length > 0) {
            //     // Selection.objects = blackBoardSelected.toInspectorBinding();
            //     return;
            // }
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
                    RecordUndo("Create Resource Node");
                    foreach (var row in rows) {
                        ResourceNodeView node = m_NodeViewModel.CreateResourceNode(row);
                        node.SetPos(mousePosition - new Vector2(50, 20) + cpt * new Vector2(0, 30));
                        ++cpt;
                    }
                }
            }
            else if (getSelectingPassScripts(out var types)) {
                DragAndDrop.AcceptDrag();
                Vector2 mousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
                var offset = new Vector2(-50, -20);
                RecordUndo("Create Pass Node");
                foreach (Type type in types) {
                    PassNodeView node = m_NodeViewModel.CreatePassNode(type);
                    node.SetPos(mousePosition + offset);
                    offset.y += node.ParameterViews.Count * 80 + 50;
                }
            }
            evt.StopPropagation();
        }
        void OnDragUpdated(DragUpdatedEvent evt) {
            // Debug.Log("OnDragUpdated...");
            if (getSelectingBlackboardField(out var _) || getSelectingPassScripts(out var _)) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                evt.StopPropagation();
            }
        }
        bool getSelectingBlackboardField(out RPGBlackboardRow[] rows) {
            rows = selection.OfType<RPGBlackboardField>().Select(t => t.GetFirstAncestorOfType<RPGBlackboardRow>()).Where(t => t != null).ToArray();
            return DragAndDrop.GetGenericData("DragSelection") != null &&
                   selection.Any(t => t is RPGBlackboardField && (t as RPGBlackboardField).GetFirstAncestorOfType<RPGBlackboardRow>() != null);
        }
        bool getSelectingPassScripts(out Type[] types) {
            Object[] objectReferences = DragAndDrop.objectReferences;
            types = null;
            if (objectReferences.All(t => t is MonoScript)) {
                MonoScript[] scripts = objectReferences.Cast<MonoScript>().ToArray();
                if (scripts.All(t => t.GetClass().IsSubclassOf(typeof(RPGPass)))) {
                    types = scripts.Select(t => t.GetClass()).ToArray();
                    return true;
                }
            }
            return false;
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
        // public void NotifySelectionPositionChange() {
        //     foreach (IRPGMovable movable in selection.OfType<IRPGMovable>()) {
        //         movable.OnMoved();
        //     }
        //     m_GraphUndoable.RecordUndo("change position");
        // }
    }
}
