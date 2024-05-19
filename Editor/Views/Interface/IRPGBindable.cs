using UnityEngine;

namespace RenderPipelineGraph.Interface {
    interface IRPGBindable {
        Object BindingObject(bool multiple=false);
    }
}
