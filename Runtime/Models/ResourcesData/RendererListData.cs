using System;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    public class RendererListData : ResourceData {
        CullingDefault m_Culling = new();
        [SerializeField] SortingCriteria m_SortingCriteria = SortingCriteria.CommonOpaque;
        [SerializeField] string m_ShaderTagIdStr = "";
        ShaderTagId m_ShaderTagId;
        [SerializeField] RenderQueueRange m_RenderQueueRange = RenderQueueRange.all;
        [SerializeField] int m_LayerMask = -1;

        public class RendererListBinding : RPGModel.RPGModelBinding {
            readonly RendererListData m_Data;
            public SortingCriteria sortingCriteria {
                get => m_Data.m_SortingCriteria;
                set => m_Data.m_SortingCriteria = value;
            }

            public RendererListBinding(RendererListData data) {
                m_Data = data;
                
            }
        }
        public RendererListData() {
            m_ObjBinding = new RendererListBinding(this);
        }
        //[SerializeField] uint m_RenderingLayerMask = 4294967295;
        public override RPGModelBinding getInspectorBinding() {
            return m_ObjBinding;
        }
    }
}
