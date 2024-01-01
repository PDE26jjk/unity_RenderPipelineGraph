using RenderPipelineGraph.Interface;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {

    public class RPGNodeEdge : Edge, IRPGDeletable {

        public RPGNodeEdge() {
            // RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
        }
        public void OnDelete() {
            // if (!isGhostEdge)
                // Debug.Log("delete edge");
        }
    }
}
