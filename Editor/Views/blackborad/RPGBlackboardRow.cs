using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public class RPGBlackboardRow : BlackboardRow{
        internal RPGBlackboardField m_Field;
        internal ResourceData model;
        internal string ResourceName {
            get => m_Field.text;
            set => m_Field.text = value;
        }
        RPGBlackboardRow(RPGBlackboardField item, VisualElement propertyView) : base(item, propertyView) {
            m_Field = item;
        }
        public RPGBlackboardRow(ResourceData model) : this(new RPGBlackboardField() { name = "rpg-field" }, new RPGBlackboardPropertyView() { name = "rpg-properties" }) {
            ResourceName = model.name;
            m_Field.typeText = Enum.GetName(typeof(ResourceType), model.type);
            this.model = model;
            Button button = this.Q<Button>("expandButton");
            if (button != null)
            {
                // button.clickable.clicked += OnExpand;
            }
        }
    }
}
