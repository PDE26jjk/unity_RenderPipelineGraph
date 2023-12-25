using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class RendererListParameterData : RPGParameterData {
        // TODO find some way to use it.
        public bool cullingWhenEmpty;
        internal RendererListParameterData() {
            m_Port = new ResourcePortData(this); 
            ((ResourcePortData)m_Port.value).resourceType = ResourceType.RendererList;
        }
        public override void LoadDataField(object passData, IBaseRenderGraphBuilder builder) {
            var rendererListData = GetValue() as RendererListData;
            builder.UseRendererList(rendererListData.rendererList);
            passTypeFieldInfo.SetValue(passData, rendererListData.rendererList);
        }
    }
}
