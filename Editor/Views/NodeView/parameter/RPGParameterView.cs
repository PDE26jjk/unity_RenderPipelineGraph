using System;
using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public abstract class RPGParameterView : VisualElement {
        readonly ParameterViewModel parameterViewModel;
        const string UXML = "UXML/RPGParameter.uxml";
        protected AttachPortView m_PortView;
        protected RPGParameterData m_Model;
        public RPGParameterData Model => m_Model;
        public AttachPortView PortView => m_PortView;
        protected VisualElement m_PortContainer;
        protected VisualElement m_Contents;
        internal DropdownField defaultValueField;
        internal RPGParameterView(ParameterViewModel parameterViewModel, RPGParameterData model) {
            m_Model = model;
            (EditorGUIUtility.Load(UXMLHelpers.PackageResourcePath + UXML) as VisualTreeAsset)?.CloneTree((VisualElement)this);
            this.parameterViewModel = parameterViewModel;
            m_PortContainer = this.Q("port");
            m_Contents = this.Q("contents");
            defaultValueField = new DropdownField("Options", new List<string> {
                "Option test",
            }, 0);
            m_Contents.Add(defaultValueField);
            defaultValueField.RegisterCallback<ChangeEvent<string>>(changeDefaultValue);
        }
        void changeDefaultValue(ChangeEvent<string> evt) {
            defaultValueField.value = evt.newValue;
        }

        internal virtual void Init() {
            if (m_PortView is not null && !Contains(m_PortView)) {
                m_PortContainer.Add(m_PortView);
                m_PortView.ConnectorText = Model.Name;
            }
            defaultValueField.label = GetDefaultValueTitle();
        }
        internal virtual string GetDefaultValueTitle() => "options";
        protected static void SetHidden(VisualElement visualElement, bool hidden = true) {
            visualElement.EnableInClassList("hide", hidden);
        }
        protected static bool IsHidden(VisualElement visualElement) {
            return visualElement.ClassListContains("hide");
        }
        internal virtual void AfterInitEdge() {
            bool useDefault = m_Model.UseDefault && !m_PortView.connected;
            SetHidden(defaultValueField,!useDefault);
            if (useDefault) {
                this.defaultValueField.choices = ResourceViewModel.GetViewModel(this).GetDefaultResourceNameList(this.m_Model.resourceType);
                this.defaultValueField.value = m_Model.DefaultResource?.name ?? "----";
            }
            UpdateContentsHidden();
        }
        protected void UpdateContentsHidden() {
            if (m_Contents.Children().All(IsHidden)) {
                SetHidden(m_Contents);
            }
        }

    }
}
