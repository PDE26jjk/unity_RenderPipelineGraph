using System;
using System.Collections.Generic;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {

    public class RPGCullingDesc : JsonObject {
        public string cullingFunctionTypeName = typeof(CullingDefault).FullName;

        public override bool Equals(object obj) {
            if (obj is RPGCullingDesc)
                return Equals((RPGCullingDesc)obj);
            return  false;
        }
        protected bool Equals(RPGCullingDesc obj) {
            return obj.cullingFunctionTypeName == cullingFunctionTypeName;
        }
    }
    public class RPGRenderListDesc : JsonObject {

        public SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
        public List<string> shaderTagIdStrs = new();
        public RenderQueueRange renderQueueRange = RenderQueueRange.all;
        public int layerMask = -1;

        public bool enableDynamicBatching = false;
        public PerObjectData perObjectData = PerObjectData.None;

        public bool useOverrideShader = false;
        [FormerlySerializedAs("overrideShaderTagIdStr")]
        public string overrideShaderPath = string.Empty;
    }
    public class RendererListData : ResourceData {
        
        public JsonData<RPGRenderListDesc> m_RenderListDesc;
        
        public JsonData<RPGCullingDesc> m_CullingDesc;
        
        [NonReorderable]
        public CullingDefault cullingFunc = null;
        
        [NonReorderable]
        public CullingResults cullingResults;
        
        [NonReorderable]
        public RendererList rendererList;
        public class RendererListBinding : RPGModel.RPGModelBinding {
            readonly RendererListData m_Data;

            public RendererListBinding(RendererListData data) {
                m_Data = data;
            }
        }
        public RendererListData() {
            m_ObjBinding = new RendererListBinding(this);
            m_RenderListDesc = new RPGRenderListDesc();
            m_CullingDesc = new RPGCullingDesc();
            this.type = ResourceType.RendererList;
        }
        //[SerializeField] uint m_RenderingLayerMask = 4294967295;
        public override RPGModelBinding getInspectorBinding() {
            return m_ObjBinding;
        }
    }
}
