using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.Inspector {
    static class BoardPreferenceHelper {
        public enum Board {
            blackboard,
            componentBoard
        }


        const string rectPreferenceFormat = "vfx-{0}-rect";
        const string visiblePreferenceFormat = "vfx-{0}-visible";


        public static bool IsVisible(Board board, bool defaultState) {
            return EditorPrefs.GetBool(string.Format(visiblePreferenceFormat, board), defaultState);
        }

        public static void SetVisible(Board board, bool value) {
            EditorPrefs.SetBool(string.Format(visiblePreferenceFormat, board), value);
        }

        public static Rect LoadPosition(Board board, Rect defaultPosition) {
            string str = EditorPrefs.GetString(string.Format(rectPreferenceFormat, board));

            Rect blackBoardPosition = defaultPosition;
            if (!string.IsNullOrEmpty(str)) {
                var rectValues = str.Split(',');

                if (rectValues.Length == 4) {
                    float x, y, width, height;
                    if (float.TryParse(rectValues[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                        float.TryParse(rectValues[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                        float.TryParse(rectValues[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width) &&
                        float.TryParse(rectValues[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height)) {
                        blackBoardPosition = new Rect(x, y, width, height);
                    }
                }
            }

            return blackBoardPosition;
        }

        public static void SavePosition(Board board, Rect r) {
            EditorPrefs.SetString(string.Format(rectPreferenceFormat, board),
                string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", r.x, r.y, r.width, r.height));
        }

        public static readonly Vector2 sizeMargin = Vector2.one * 30;

        public static bool ValidatePosition(GraphElement element, RPGView view, Rect defaultPosition) {
            Rect viewrect = view.contentRect;
            Rect rect = element.GetPosition();
            bool changed = false;

            if (!viewrect.Contains(rect.position)) {
                Vector2 newPosition = defaultPosition.position;
                if (!viewrect.Contains(defaultPosition.position)) {
                    newPosition = sizeMargin;
                }

                rect.position = newPosition;

                changed = true;
            }

            Vector2 maxSizeInView = viewrect.max - rect.position - sizeMargin;
            float newWidth = Mathf.Max(element.resolvedStyle.minWidth.value, Mathf.Min(rect.width, maxSizeInView.x));
            float newHeight = Mathf.Max(element.resolvedStyle.minHeight.value, Mathf.Min(rect.height, maxSizeInView.y));

            if (Mathf.Abs(newWidth - rect.width) > 1) {
                rect.width = newWidth;
                changed = true;
            }

            if (Mathf.Abs(newHeight - rect.height) > 1) {
                rect.height = newHeight;
                changed = true;
            }

            if (changed) {
                element.SetPosition(rect);
            }

            return false;
        }
    }

    class InspectorView : GraphElement {
        RPGView m_View;
        private VisualElement m_MainContainer;
        private VisualElement m_Root;
        private Label m_TitleLabel;
        private ScrollView m_ScrollView;
        private VisualElement m_ContentContainer;
        private VisualElement m_HeaderItem;
        private bool m_Scrollable = true;
        private Dragger m_Dragger;
        internal static readonly string StyleSheetPath = "StyleSheets/GraphView/Blackboard.uss";
        public override string title {
            get => this.m_TitleLabel.text;
            set => this.m_TitleLabel.text = value;
        }


        public bool scrollable {
            get => this.m_Scrollable;
            set {
                if (this.m_Scrollable == value)
                    return;
                this.m_Scrollable = value;
                if (this.m_Scrollable) {
                    if (this.m_ScrollView == null)
                        this.m_ScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                    this.m_ContentContainer.RemoveFromHierarchy();
                    this.m_Root.Add((VisualElement)this.m_ScrollView);
                    this.m_ScrollView.Add(this.m_ContentContainer);
                    // this.resizeRestriction = ResizeRestriction.None;
                    this.AddToClassList(nameof(scrollable));
                }
                else {
                    if (this.m_ScrollView != null) {
                        // this.resizeRestriction = ResizeRestriction.FlexDirection;
                        this.m_ScrollView.RemoveFromHierarchy();
                        this.m_ContentContainer.RemoveFromHierarchy();
                        this.m_Root.Add(this.m_ContentContainer);
                    }
                    this.RemoveFromClassList(nameof(scrollable));
                }
            }
        }
        static readonly Rect defaultRect = new Rect(200, 100, 300, 300);
        public InspectorView(RPGView view) {
            m_View = view;
            this.AddStyleSheetPath("RPGBlackboard.uss");
            var tql =  Resources.Load("UXML/RPGInspector")  as VisualTreeAsset;
            this.m_MainContainer = tql.Instantiate();
            this.m_MainContainer.AddToClassList("mainContainer");
            this.m_Root = this.m_MainContainer.Q("content", (string)null);
            this.m_HeaderItem = this.m_MainContainer.Q("header", (string)null);
            this.m_HeaderItem.AddToClassList("blackboardHeader");

            this.m_TitleLabel = this.m_MainContainer.Q<Label>("titleLabel", (string)null);

            this.m_ContentContainer = this.m_MainContainer.Q(nameof(contentContainer), (string)null);
            this.hierarchy.Add(this.m_MainContainer);
            this.capabilities |= Capabilities.Resizable | Capabilities.Movable;
            this.style.overflow = (StyleEnum<Overflow>)Overflow.Hidden;
            this.ClearClassList();
            this.AddToClassList("blackboard");
            this.m_Dragger = new Dragger() {
                clampToParentEdges = true
            };
            this.AddManipulator(this.m_Dragger);
            scrollable = false;
            hierarchy.Add(new Resizer());
            RegisterCallback((EventCallback<DragUpdatedEvent>)(e => e.StopPropagation()));
            RegisterCallback((EventCallback<WheelEvent>)(e => e.StopPropagation()));

            this.focusable = true;
            SetPosition(BoardPreferenceHelper.LoadPosition(BoardPreferenceHelper.Board.componentBoard, defaultRect));
        }
        public void AddToSelection(ISelectable selectable) {
            throw new System.NotImplementedException();
        }
        public void RemoveFromSelection(ISelectable selectable) {
            throw new System.NotImplementedException();
        }

        public List<ISelectable> selection { get; }

    }
}
