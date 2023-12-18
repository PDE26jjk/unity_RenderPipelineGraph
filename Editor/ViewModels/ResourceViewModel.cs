using System;
using System.Collections.Generic;
using RenderPipelineGraph.Editor.Views.blackborad;

namespace RenderPipelineGraph {
    public class ResourceViewModel {
        Dictionary<ResourceData, RPGBlackboardRow> m_Resources = new();
        RPGView m_GraphView;
        public ResourceViewModel(RPGView graphView) {
            this.m_GraphView = graphView;
        }
        public IEnumerable<RPGBlackboardRow> LoadResources() {
            RPGAsset asset = m_GraphView.Asset;
            foreach (ResourceData resourceData in asset.ResourceList) {

                var row = new RPGBlackboardRow(resourceData);
                row.m_Field.typeText = Enum.GetName(typeof(ResourceType), resourceData.type);
                row.m_Field.text = resourceData.name;
                m_Resources[resourceData] = row;
                yield return row;
            }
            yield break;
        }
    }
}
