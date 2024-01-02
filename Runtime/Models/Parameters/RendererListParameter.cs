using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class RendererListParameterData : RPGParameterData {
        // TODO find a way to use it.
        public bool cullingWhenEmpty;

        internal RendererListParameterData(FieldInfo fieldInfo) : base(fieldInfo) {
            m_Port.value.resourceType = ResourceType.RendererList;
        }
        public override void Init() {
            base.Init();
            if (customAttributes.Contains(typeof(CullingWhenEmptyAttribute))) {
                cullingWhenEmpty = true;
            }
        }
        public override void LoadDataField(object passData, IBaseRenderGraphBuilder builder) {
            if (GetValue() is not RendererListData rendererListData) {
                Debug.LogError($"RendererList error: {Name} cannot load.");
                return; 
            }
            builder.UseRendererList(rendererListData.rendererList); 
            passTypeFieldInfo.SetValue(passData, rendererListData.rendererList);
        }
        
        RendererListParameterData(){}
    }
}
