using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {

    public class RPGNodeEdge : Edge {

        public RPGNodeEdge() {
            RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
        }
        void OnLeavePanel(DetachFromPanelEvent evt) {
            if (this.input is not null && this.output is not null) {
                Debug.Log("edge delete");
            }
        }
        // void Dispose() {
        //     
        //     Debug.Log("Dispose edge");
        //     UnregisterCallback<ExecuteCommandEvent>(ExecuteCommand);
        // }

    }
}
