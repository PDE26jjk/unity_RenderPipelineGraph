using System;
using System.Collections.Generic;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    [Serializable]
    public class RenderQueueRangeClass {
        public int LowerBound;
        public int UpperBound;
        public RenderQueueRangeClass(RenderQueueRange renderQueueRange) {
            LowerBound = renderQueueRange.lowerBound;
            UpperBound = renderQueueRange.upperBound;
        }
        public static implicit operator RenderQueueRangeClass(RenderQueueRange renderQueueRange)
        {
            return new RenderQueueRangeClass(renderQueueRange);
        }
        
        public static implicit operator RenderQueueRange(RenderQueueRangeClass cla)
        {
            return new RenderQueueRange(cla.LowerBound,cla.UpperBound);
        }
        
    }
    public class RPGRenderListDesc : JsonObject {

        public SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
        public List<string> shaderTagIdStrs = new();
        
        public RenderQueueRangeClass renderQueueRange = RenderQueueRange.all;
        public int layerMask = -1;

        public bool enableDynamicBatching = false;
        public PerObjectData perObjectData = PerObjectData.None;

        public bool useOverrideShader = false;
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
        public RendererListHandle rendererList;
        // public class RendererListBinding : RPGModel.RPGModelBinding {
        //     readonly RendererListData m_Data;
        //
        //     public RendererListBinding(RendererListData data) {
        //         m_Data = data;
        //     }
        // }
        public RendererListData() {
            // m_ObjBinding = new RendererListBinding(this);
            m_RenderListDesc = new RPGRenderListDesc();
            m_CullingDesc = new RPGCullingDesc();
            this.type = ResourceType.RendererList;
        }
        //[SerializeField] uint m_RenderingLayerMask = 4294967295;
        // public override RPGModelBinding getInspectorBinding() {
        //     return m_ObjBinding;
        // }
    }
}
