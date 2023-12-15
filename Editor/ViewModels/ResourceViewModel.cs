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

                RPGBlackboardRow row = null;
                row = new RPGBlackboardRow(resourceData);
                
                // switch (resourceData.type) {
                //     case ResourceType.Texture:
                //         row.m_Field.typeText = "Texture";
                //         break;
                //     case ResourceType.Buffer:
                //         row.m_Field.typeText = "Buffer";
                //         break;
                //     case ResourceType.AccelerationStructure:
                //         row.m_Field.typeText = "Texture";
                //         break;
                //     default:
                //         throw new ArgumentOutOfRangeException();
                // }
                row.m_Field.typeText = Enum.GetName(typeof(ResourceType), resourceData.type);
                row.m_Field.text = resourceData.name;
                m_Resources[resourceData] = row;
                yield return row;
            }
            yield break;
        }
    }
}
