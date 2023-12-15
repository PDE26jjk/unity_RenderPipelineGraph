using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public class RPGBlackboardRow : BlackboardRow{
        internal RPGBlackboardField m_Field;
        internal ResourceData model;
        RPGBlackboardRow(RPGBlackboardField item, VisualElement propertyView) : base(item, propertyView) {
            m_Field = item;
        }
        public RPGBlackboardRow(ResourceData model) : this(new RPGBlackboardField() { name = "rpg-field" }, new RPGBlackboardPropertyView() { name = "rpg-properties" }) {
            this.model = model;
            Button button = this.Q<Button>("expandButton");

            if (button != null)
            {
                // button.clickable.clicked += OnExpand;
            }
        }
    }
}
