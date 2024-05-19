using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Interface;
using UnityEngine;

namespace RenderPipelineGraph {
    public partial class ResourceNodeView :IRPGBindable{
        
        public Object BindingObject(bool multiple=false) {
            RPGBlackboardRow rpgBlackboardRow = GetBlackboardRow();
            return rpgBlackboardRow?.BindingObject(multiple);
        }
    }
}
