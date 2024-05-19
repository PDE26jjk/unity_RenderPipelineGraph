using System;
using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public partial class RPGBlackboardRow : IRPGBindable {
        public class RendererListBinding : ResourceDataBinding {
            public SortingCriteria sortingCriteria;
            public List<string> shaderTagIds;
            public RenderQueueRange renderQueueRange;
            public int renderQueueRangeLowerBound;
            public int renderQueueRangeUpperBound;
            public int layerMask;

            public bool enableDynamicBatching;
            public PerObjectData perObjectData;

            public bool useOverrideShader;
            public string overrideShaderPath;
            public string cullingFunctionTypeName;
            public override void Init(ResourceData model) {
                base.Init(model);
                if (Model is RendererListData data) {
                    LoadDesc(data.m_RenderListDesc.value, data.m_CullingDesc.value);
                }
            }
            void LoadDesc(RPGRenderListDesc desc1, RPGCullingDesc desc2) {
                sortingCriteria = desc1.sortingCriteria;
                shaderTagIds = desc1.shaderTagIdStrs;
                renderQueueRange = desc1.renderQueueRange;
                renderQueueRangeLowerBound = renderQueueRange.lowerBound;
                renderQueueRangeUpperBound = renderQueueRange.upperBound;
                layerMask = desc1.layerMask;
                enableDynamicBatching = desc1.enableDynamicBatching;
                perObjectData = desc1.perObjectData;
                useOverrideShader = desc1.useOverrideShader;
                overrideShaderPath = desc1.overrideShaderPath;
                cullingFunctionTypeName = desc2.cullingFunctionTypeName;
            }
        }

        [CustomEditor(typeof(RendererListBinding))]
        public class RendererListBindingEditor : RPGEditorBase {
            public override VisualElement CreateInspectorGUI() {
                var root = new VisualElement();
                if (!CheckAndAddNameField(root, out RendererListBinding rendererListBinding, out RendererListData rendererListData)) {
                    return root;
                }
                var descData = rendererListData.m_RenderListDesc.value;
                VisualElement sortingCriteria = CreatePropertyField<SortingCriteria>("sortingCriteria",descData);
                
                VisualElement renderQueueRangeLowerBound = CreatePropertyField<int>("renderQueueRangeLowerBound", () => {
                    descData.renderQueueRange.LowerBound = rendererListBinding.renderQueueRangeLowerBound;
                });
                VisualElement renderQueueRangeUpperBound = CreatePropertyField<int>("renderQueueRangeUpperBound", () => {
                    descData.renderQueueRange.UpperBound = rendererListBinding.renderQueueRangeUpperBound;
                });
                root.Add(renderQueueRangeLowerBound);
                root.Add(renderQueueRangeUpperBound);
                root.Add(sortingCriteria);
                VisualElement shaderTagIds = CreatePropertyField<List<string>>("shaderTagIds",descData,"shaderTagIdStrs");
                root.Add(shaderTagIds);
                return root;
            }

        }

    }
}
