using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RenderPipelineGraph.Editor.Views.Inspector;
using RenderPipelineGraph.Editor.Views.NodeView;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class RPGView : GraphView {

    SelectionDragger m_SelectionDragger;
    RectangleSelector m_RectangleSelector;
    // internal ICollection<testGraphPort> getPorts() {
    //     return Ports.AsReadOnlyCollection();
    // }
    InspectorView m_Inspector;
    public RPGView() {
        SetupZoom(0.125f, 8);
        m_Blackboard = new RPGBlackboard(this);
        m_Inspector = new InspectorView(this);
        // bool blackboardVisible = BoardPreferenceHelper.IsVisible(BoardPreferenceHelper.Board.blackboard, true);
        // if (blackboardVisible)
        Add(m_Blackboard);

        Add(m_Inspector);

        this.AddManipulator(new ContentDragger());
        m_SelectionDragger = new SelectionDragger();
        m_RectangleSelector = new RectangleSelector();
        this.AddManipulator(m_SelectionDragger);
        this.AddManipulator(m_RectangleSelector);
        this.AddManipulator(new FreehandSelector());

        AddLayer(-1);
        AddLayer(1);
        AddLayer(2);
        focusable = true;

        m_Toolbar = new UnityEditor.UIElements.Toolbar();
        Add(m_Toolbar);

        RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        RegisterCallback<DragPerformEvent>(OnDragPerform);
        RegisterCallback<ValidateCommandEvent>(ValidateCommand);
        RegisterCallback<ExecuteCommandEvent>(ExecuteCommand);
        RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
        RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
        RegisterCallback<KeyDownEvent>(OnKeyDownEvent);
        RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);

        var node1 = new RPGNode {
            focusable = true,
            tabIndex = 0,
            usageHints = UsageHints.None,
            name = "node1",
            languageDirection = LanguageDirection.Inherit,
            visible = true,
            generateVisualContent = null,
            dataSource = null,
            dataSourcePath = default,
            tooltip = null,
            elementTypeColor = default,
            layer = -1,
            title = "title1",
        };
        var node2 = new RPGNode();
        node1.SetPosition(new Rect(100, 100, 0, 0));
        node1.SetPosition(new Rect(100, 200, 0, 0));
        FastAddElement(node1);
        FastAddElement(node2);
    }
    public void SetBoardToFront(GraphElement board) {
        board.SendToBack();
        board.PlaceBehind(m_Toolbar);
    }
    void OnMouseMoveEvent(MouseMoveEvent evt) {
        Debug.Log("OnMouseMoveEvent");
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
    }
    void ValidateCommand(ValidateCommandEvent evt) {
        Debug.Log("ValidateCommand");
    }
    void OnDragPerform(DragPerformEvent evt) {
        Debug.Log("OnDragPerform");
    }
    void OnDragUpdated(DragUpdatedEvent evt) {
        Debug.Log("OnDragUpdated");
    }
    static FieldInfo s_Member_ContainerLayer = typeof(GraphView).GetField("m_ContainerLayers", BindingFlags.NonPublic | BindingFlags.Instance);
    static MethodInfo s_Method_GetLayer = typeof(GraphView).GetMethod("GetLayer", BindingFlags.NonPublic | BindingFlags.Instance);
    private Toolbar m_Toolbar;
    readonly RPGBlackboard m_Blackboard;

    public void FastAddElement(GraphElement graphElement) {
        if (graphElement.IsResizable()) {
            graphElement.hierarchy.Add(new Resizer());
            graphElement.style.borderBottomWidth = 6;
        }

        int newLayer = graphElement.layer;
        if (!(s_Member_ContainerLayer.GetValue(this) as IDictionary).Contains(newLayer)) {
            AddLayer(newLayer);
        }
        (s_Method_GetLayer.Invoke(this, new object[] {
            newLayer
        }) as VisualElement).Add(graphElement);
    }


    public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter) {
        List<Port> list = new();
        if (startAnchor is not RPGPort port) {
            return list;
        }
        foreach (var n in this.nodes.ToList()) {
            if (n is RPGNode node) {
                node.GetCompatiblePorts(list, port);
            }
        }
        return list;
    }

}
