using UnityEditor.Experimental.GraphView;

namespace RenderPipelineGraph {
    public class TextureParamView:RPGParameterView {

        internal TextureParamView(ParameterViewModel parameterViewModel,TextureParameterData model) : base(parameterViewModel,model) {
            m_PortView = new AttachPortView(Direction.Input, typeof(TextureParameterData));
        }
        internal override string GetDefaultValueTitle() => "Default Texture";
    }
}
