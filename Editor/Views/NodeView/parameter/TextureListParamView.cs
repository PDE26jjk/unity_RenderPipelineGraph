using UnityEditor.Experimental.GraphView;

namespace RenderPipelineGraph {
    public class TextureListParamView:RPGParameterView {

        internal TextureListParamView(ParameterViewModel parameterViewModel,TextureListParameterData model) : base(parameterViewModel,model) {
            m_PortView = new AttachPortView(Direction.Input, typeof(TextureListParameterData));
        }
        internal override string GetDefaultValueTitle() => "Default TextureList";
    }
}
