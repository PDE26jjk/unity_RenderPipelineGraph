using System.Reflection;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class CullingResultParameterData : RPGParameterData {
        internal CullingResultParameterData(FieldInfo fieldInfo):base(fieldInfo) {
            UseDefault = true;
            m_Port.value.resourceType = ResourceType.CullingResult;
        }
        public override bool NeedPort() {
            return false;
        }
        public override void LoadDataField(object passData, IBaseRenderGraphBuilder builder) {
            var cullingResultData = GetValue() as CullingResultData;
            passTypeFieldInfo.SetValue(passData, cullingResultData.cullingResults);
        }
        
        CullingResultParameterData(){}

    }
}
