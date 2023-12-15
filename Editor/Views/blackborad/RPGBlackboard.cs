using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using PositionType = UnityEngine.UIElements.Position;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public class RPGBlackboard : Blackboard {
        RPGView m_View;
        readonly Button m_AddButton;
        readonly VisualElement m_ContentContainer;
        VisualElement m_DragIndicator;
        bool m_CanEdit;
        RPGBlackboardCategory m_DefaultCategory;
        RPGBlackboardCategory m_OutputCategory;
        Dictionary<string, RPGBlackboardCategory> m_Categories = new();
        static System.Reflection.PropertyInfo s_LayoutManual =
            typeof(VisualElement).GetProperty("isLayoutManual", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        static readonly Rect defaultRect = new Rect(100, 100, 300, 500);
        internal readonly ResourceViewModel m_ResourceViewModel;
        public RPGBlackboard(RPGView view) {
            this.m_View = view;
            this.m_ResourceViewModel = new ResourceViewModel(view);
            m_DefaultCategory = new RPGBlackboardCategory();
            Add(m_DefaultCategory);
            // m_DefaultCategory.headerVisible = false;

            editTextRequested = OnEditName;
            addItemRequested = OnAddItem;
            
            RegisterCallback<MouseDownEvent>(OnMouseClick);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
            RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            // m_DefaultCategory = new VFXBlackboardCategory() { title = "parameters" };
            this.scrollable = true;
            focusable = true;
            m_ContentContainer = this.Q<VisualElement>("contentContainer");

            m_AddButton = this.Q<Button>(name: "addButton");
            m_AddButton.style.width = 27;
            m_AddButton.style.height = 27;
            m_AddButton.SetEnabled(true);
            m_DragIndicator = new VisualElement();
            m_DragIndicator.name = "dragIndicator";
            m_DragIndicator.style.position = PositionType.Absolute;
            hierarchy.Add(m_DragIndicator);

            SetDragIndicatorVisible(false);

            Resizer resizer = this.Query<Resizer>();

            hierarchy.Add(new UnityEditor.Experimental.GraphView.ResizableElement());

            resizer.RemoveFromHierarchy();

            if (s_LayoutManual != null)
                s_LayoutManual.SetValue(this, false);

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            var scrollView = this.Q<ScrollView>();
            if (scrollView != null) {
                scrollView.RegisterCallback<GeometryChangedEvent, ScrollView>(OnGeometryChanged, scrollView);
                scrollView.horizontalScroller.valueChanged += x => OnOutputCategoryScrollChanged(scrollView);
            }
            
            foreach (RPGBlackboardRow row in m_ResourceViewModel.LoadResources()) {
                m_DefaultCategory.Add(row);
            }

        }
        void OnKeyDown(KeyDownEvent evt) {
            // throw new System.NotImplementedException();
        }
        void OnDragLeaveEvent(DragLeaveEvent evt) {
            // throw new System.NotImplementedException();
        }
        void OnDragPerformEvent(DragPerformEvent evt) {
            // throw new System.NotImplementedException();
        }
        int InsertionIndex(Vector2 pos) {
            VisualElement owner = contentContainer != null ? contentContainer : this;
            Vector2 localPos = this.ChangeCoordinatesTo(owner, pos);

            if (owner.ContainsPoint(localPos)) {
                int defaultCatIndex = IndexOf(m_DefaultCategory);

                for (int i = defaultCatIndex + 1; i < childCount; ++i) {
                    var cat = ElementAt(i) as RPGBlackboardCategory;
                    if (cat == null) {
                        return i;
                    }

                    Rect rect = cat.layout;

                    if (localPos.y <= (rect.y + rect.height / 2)) {
                        return i;
                    }
                }
                return childCount;
            }
            return -1;
        }

        int m_InsertIndex;
        void OnDragUpdatedEvent(DragUpdatedEvent e) {
            var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            if (selection == null) {
                SetDragIndicatorVisible(false);
                return;
            }

            if (selection.Any(t => !(t is RPGBlackboardCategory))) {
                SetDragIndicatorVisible(false);
                return;
            }

            Vector2 localPosition = e.localMousePosition;

            m_InsertIndex = InsertionIndex(localPosition);

            if (m_InsertIndex != -1) {
                float indicatorY = 0;

                if (m_InsertIndex == childCount) {
                    if (childCount > 0) {
                        VisualElement lastChild = this[childCount - 1];

                        indicatorY = lastChild.ChangeCoordinatesTo(this, new Vector2(0, lastChild.layout.height + lastChild.resolvedStyle.marginBottom)).y;
                    }
                    else {
                        indicatorY = this.contentRect.height;
                    }
                }
                else {
                    VisualElement childAtInsertIndex = this[m_InsertIndex];

                    indicatorY = childAtInsertIndex.ChangeCoordinatesTo(this, new Vector2(0, -childAtInsertIndex.resolvedStyle.marginTop)).y;
                }

                SetDragIndicatorVisible(true);

                m_DragIndicator.style.top = indicatorY - m_DragIndicator.resolvedStyle.height * 0.5f;

                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
            else {
                SetDragIndicatorVisible(false);
            }
            e.StopPropagation();
        }
        void OnMouseClick(MouseDownEvent evt) {
            m_View.SetBoardToFront(this);
        }
        void OnAddItem(Blackboard obj) {
            // if (!m_CanEdit)
            // {
            //     return;
            // }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(EditorGUIUtility.TrTextContent("Category"), false, OnAddCategory);
            menu.AddSeparator(string.Empty);

            menu.AddItem(EditorGUIUtility.TrTextContent("Texture"), false, OnAddParameter, null);
            menu.ShowAsContext();
        }
        void OnAddCategory() {
            string initialName = "new category";
            var newCategoryName = initialName;

            // controller.graph.UIInfos.categories ??= new List<VFXUI.CategoryInfo>();
            // controller.graph.UIInfos.categories.Add(new VFXUI.CategoryInfo { name = newCategoryName });
            // controller.graph.Invalidate(VFXModel.InvalidationCause.kUIChanged);
            //
            // return newCategoryName;
        }
        void OnAddParameter(object parameter) {
            var selectedCategory = m_View.selection.OfType<RPGBlackboardCategory>().FirstOrDefault();
            selectedCategory ??= m_DefaultCategory;
            RPGView graphView = GetFirstAncestorOfType<RPGView>();
            // graphView.m_ViewModel

            var row = new RPGBlackboardRow(ScriptableObject.CreateInstance<TextureData>());
            row.name = "tete";
            selectedCategory.Add(row);
            
            // VFXParameter newParam = m_Controller.AddVFXParameter(Vector2.zero, (VFXModelDescriptorParameters)parameter);
            // if (selectedCategory != null && newParam != null)
            //     newParam.category = selectedCategory.title;

            // newParam.SetSettingValue("m_Exposed", true);
        }
        void OnEditName(Blackboard arg1, VisualElement arg2, string arg3) {
            Debug.Log("OnEditName");
        }
        void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            evt.menu.AppendAction("Select All", (a) => SelectAll(), (a) => GetContextualMenuStatus());
            evt.menu.AppendAction("Select Unused", (a) => SelectUnused(), (a) => GetContextualMenuStatus());
        }
        void OnGeometryChanged(GeometryChangedEvent evt, ScrollView scrollView) {
            if (scrollView != null) {
                var addOutputButton = scrollView.Q<Button>("addOutputButton");
                if (addOutputButton != null) {
                    addOutputButton.style.left = -scrollView.horizontalScroller.highValue + scrollView.horizontalScroller.value;
                }
            }
        }
        DropdownMenuAction.Status GetContextualMenuStatus() {
            //Use m_AddButton state which relies on locked & controller status
            return m_AddButton.enabledSelf && m_CanEdit ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
        }
        void OnOutputCategoryScrollChanged(ScrollView scrollView) {
            OnGeometryChanged(null, scrollView);
        }
        void SelectAll() {
            m_View.ClearSelection();
            this.Query<BlackboardField>().ForEach(t => m_View.AddToSelection(t));
        }
        void SelectUnused() {
            m_View.ClearSelection();


        }

        private void SetDragIndicatorVisible(bool visible) {
            if (visible && (m_DragIndicator.parent == null)) {
                hierarchy.Add(m_DragIndicator);
                m_DragIndicator.visible = true;
            }
            else if ((visible == false) && (m_DragIndicator.parent != null)) {
                hierarchy.Remove(m_DragIndicator);
            }
        }

    }
}