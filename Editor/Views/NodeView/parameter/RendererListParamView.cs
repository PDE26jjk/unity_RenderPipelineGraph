using UnityEditor.Experimental.GraphView;

namespace RenderPipelineGraph {
    public class RendererListParamView : RPGParameterView {

        internal RendererListParamView(ParameterViewModel parameterViewModel,RendererListParameterData model) : base(parameterViewModel,model) {
            m_PortView = new AttachPortView(Direction.Input, typeof(RendererListParameterData));
        }
        internal override string GetDefaultValueTitle() => "Default RenderList";
    }
}
