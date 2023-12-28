using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace RenderPipelineGraph {
    public class cullingResultParamView:RPGParameterView {
        internal cullingResultParamView(ParameterViewModel parameterViewModel, CullingResultParameterData model) : base(parameterViewModel,model) {
            m_PortView = new AttachPortView(Direction.Input, typeof(CullingResultParameterData));
        }

        internal override string GetDefaultValueTitle() => "Culling Method";
    }
}
