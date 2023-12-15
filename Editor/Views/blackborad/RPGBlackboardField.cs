using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public class RPGBlackboardField : BlackboardField{
        public RPGBlackboardField() : base()
        {
            this.Q<Pill>().AddManipulator(new ContextualMenuManipulator(PillBuildContextualMenu));
            // text = "111";
            // typeText = "222";
        }
        void PillBuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            // evt.menu.AppendAction("Duplicate %d", (a) => Duplicate(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", (a) => Delete(), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
        void Delete()
        {
            if (selected)
                GetFirstAncestorOfType<RPGView>().DeleteSelection();
            else
                GetFirstAncestorOfType<RPGView>().DeleteElements(new GraphElement[] { this });
        }
    }
}
