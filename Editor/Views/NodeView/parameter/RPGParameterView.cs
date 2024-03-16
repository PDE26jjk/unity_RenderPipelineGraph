using System;
using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public abstract class RPGParameterView : VisualElement {
        internal readonly ParameterViewModel parameterViewModel;
        const string UXML = "UXML/RPGParameter.uxml";
        protected AttachPortView m_PortView;
        protected RPGParameterData m_Model;
        public RPGParameterData Model => m_Model;
        public AttachPortView PortView => m_PortView;
        protected VisualElement m_PortContainer;
        protected VisualElement m_Contents;
        internal DropdownField defaultValueField;

        internal bool inited = false;
        protected VisualElement m_Fields;
        internal RPGParameterView(ParameterViewModel parameterViewModel, RPGParameterData model) {
            m_Model = model;
            (EditorGUIUtility.Load(UXMLHelpers.PackageResourcePath + UXML) as VisualTreeAsset)?.CloneTree((VisualElement)this);
            this.parameterViewModel = parameterViewModel;
            m_PortContainer = this.Q("port");
            m_Contents = this.Q("contents");
            m_Fields = new VisualElement();
            defaultValueField = new DropdownField("Options", new List<string> {
                "Option test",
            }, 0);
            m_Fields.Add(defaultValueField);
            m_Contents.Add(m_Fields);
            defaultValueField.RegisterCallback<ChangeEvent<string>>(changeDefaultValue);
        }
        void changeDefaultValue(ChangeEvent<string> evt) {
            defaultValueField.value = evt.newValue;
            if (inited) {
                GetFirstAncestorOfType<RPGView>().RecordUndo($"Change default Value of {parameterViewModel.NodeView.title}.{Model.Name}");
                OnDefaultValueChange(defaultValueField.value);
            }
            inited = true;
        }

        internal virtual void Init() {
            if (m_PortView is not null && !Contains(m_PortView)) {
                m_PortContainer.Add(m_PortView);
                m_PortView.ConnectorText = Model.Name;
            }
            defaultValueField.label = GetDefaultValueTitle();
        }
        internal virtual string GetDefaultValueTitle() => "options";

        internal virtual bool IsSomethingWrong() {
            return !m_Model.UseDefault && !m_PortView.connected;
        }

        internal virtual void AfterInitEdge() {
            bool useDefault = m_Model.UseDefault && !m_PortView.connected;
            ShowDropdownField(useDefault);
            if (parameterViewModel.GetNodeViewModel().Loading) 
                inited = true;
        }
        protected virtual void ShowDropdownField(bool show) {
            defaultValueField.SetDisplay(show);
            if (show) {
                SetupDefaultValue();
            }
            UpdateContentsHidden();
        }
        void SetupDefaultValue() {
            this.defaultValueField.choices = ResourceViewModel.GetViewModel(this).GetDefaultResourceNameList(this.m_Model.resourceType);
            this.defaultValueField.value = m_Model.DefaultResource?.name ?? "----";
        }
        public void OnDefaultValueChange(string defaultValueName) {
            this.NotifyDefaultValueChangeVM(defaultValueName);
        }
        protected void UpdateContentsHidden() {
            if (m_Fields.Children().All(t=>!t.IsDisplay())) {
                m_Contents.SetDisplay(false);
            }
            else {
                m_Contents.SetDisplay(true);
            }
        }

        public void NotifyDisconnectPort(Edge edge) {
            this.NotifyDisconnectPortVM(edge);
            ShowDropdownField(true);
        }
        public void NotifyConnectPort(Edge edge) {
            this.NotifyConnectPortVM(edge);
            ShowDropdownField(false);
        }
    }
}
