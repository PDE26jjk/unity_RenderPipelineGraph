using UnityEditor.Experimental.GraphView;

namespace RenderPipelineGraph.Editor.Interfaces
{
    interface IRPGResizable : IResizable
    {
        bool CanResizePastParentBounds();
    }
}
