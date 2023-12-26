﻿using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Attribute;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class RendererListParameterData : RPGParameterData {
        // TODO find some way to use it.
        public bool cullingWhenEmpty;

        public override void Init() {
            base.Init();
            if (customAttributes.Contains(typeof(CullingWhenEmptyAttribute))) {
                cullingWhenEmpty = true;
            }
        }
        internal RendererListParameterData(FieldInfo fieldInfo) : base(fieldInfo) {
            m_Port.value.resourceType = ResourceType.RendererList;
        }
        public override void LoadDataField(object passData, IBaseRenderGraphBuilder builder) {
            var rendererListData = GetValue() as RendererListData;
            builder.UseRendererList(rendererListData.rendererList);
            passTypeFieldInfo.SetValue(passData, rendererListData.rendererList);
        }
        
        RendererListParameterData(){}
    }
}
