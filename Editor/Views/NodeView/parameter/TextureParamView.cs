using System.Collections.Generic;
using RenderPipelineGraph.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class TextureParamView : RPGParameterView {

        protected IntegerField listIndexField;
        internal TextureParamView(ParameterViewModel parameterViewModel, TextureParameterData model) : base(parameterViewModel, model) {
            m_PortView = new AttachPortView(Direction.Input, typeof(TextureParameterData));
            listIndexField = new IntegerField("index");
            listIndexField.value = model.listIndex;
            listIndexField.RegisterCallback<ChangeEvent<int>>(ChangeListIndex);
            m_Fields.Add(listIndexField);
        }
        void ChangeListIndex(ChangeEvent<int> evt) {
            listIndexField.value = evt.newValue;
            if (inited) {
                GetFirstAncestorOfType<RPGView>().RecordUndo($"Change List Index of {parameterViewModel.NodeView.title}.{Model.Name}");
                this.NotifyListIndexChangeVM(listIndexField.value);
            }
            inited = true;
        }
        protected override void ShowDropdownField(bool show) {
            this.listIndexField.SetDisplay(this.Model.GetValue() is TextureListData);
            base.ShowDropdownField(show);
        }
        internal override string GetDefaultValueTitle() => "Default Texture";
    }
}
