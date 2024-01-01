using RenderPipelineGraph.Interface;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public partial class RPGBlackboardField : BlackboardField, IRPGDeletable {
        public RPGBlackboardField() : base() {
            this.Q<Pill>().AddManipulator(new ContextualMenuManipulator(PillBuildContextualMenu));
        }
        void PillBuildContextualMenu(ContextualMenuPopulateEvent evt) {
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            // evt.menu.AppendAction("Duplicate %d", (a) => Duplicate(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", (a) => DeleteAction(), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
        void DeleteAction() {
            RPGView view = GetFirstAncestorOfType<RPGView>();
            if (selected)
                view?.DeleteSelection();
            else
                view?.DeleteElements(new GraphElement[] {
                    this
                });
        }

        public void OnDelete() {
            GetFirstAncestorOfType<RPGBlackboardRow>().OnDelete();
        }
    }
}
