using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using RenderPipelineGraph.Editor;
using RenderPipelineGraph.Editor.Views.blackborad;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    //every graph has one of it.
    public class ResourceViewModel {
        public static ResourceViewModel GetViewModel(VisualElement visualElement) {
            RPGView graphView = visualElement.GetFirstAncestorOfType<RPGView>();
            return graphView.m_ResourceViewModel;
        }
        Dictionary<ResourceData, RPGBlackboardRow> m_Resources = new();
        RPGView m_GraphView;
        public ResourceViewModel(RPGView graphView) {
            this.m_GraphView = graphView;
        }
        // blackboard use it to load resources.
        public IEnumerable<Tuple<string, RPGBlackboardRow>> LoadResources() {
            foreach (ResourceData resourceData in m_GraphView.currentGraph.ResourceList) {
                var row = new RPGBlackboardRow(resourceData);
                m_Resources[resourceData] = row;
                yield return new Tuple<string, RPGBlackboardRow>(resourceData.category, row);
            }
            yield break;
        }
        internal bool needUpdateDefaultResourceList = true;
        List<RPGBlackboardRow>[] DefaultResourceLists = new List<RPGBlackboardRow>[(int)ResourceType.Count];
        List<string>[] DefaultResourceStrLists = new List<string>[(int)ResourceType.Count];
        public List<string> GetDefaultResourceNameList(ResourceType resourceType) {
            if (needUpdateDefaultResourceList) {
                needUpdateDefaultResourceList = false;
                var defaultResourceDatas = m_Resources.Keys.Where(t=>t.isDefault);
                for (int i = 0; i < (int)ResourceType.Count; i++) {
                    DefaultResourceLists[i] ??= new();
                    DefaultResourceLists[i] = defaultResourceDatas.Where(t => t.type == (ResourceType)i).Select(t => m_Resources[t]).ToList();
                    DefaultResourceStrLists[i] ??= new();
                    DefaultResourceStrLists[i] = DefaultResourceLists[i].Select(t => t.ResourceName).ToList();
                }
            }
            return DefaultResourceStrLists[(int)resourceType];
        }
        public RPGBlackboardRow CreateResource(string name, ResourceType resourceType, RPGBlackboardCategory blackboardCategory) {
            ResourceData resourceData = resourceType switch {
                ResourceType.Texture => new TextureData(),
                ResourceType.Buffer => new BufferData(),
                ResourceType.AccelerationStructure => new RTAData(),
                ResourceType.RendererList => new RendererListData(),
                ResourceType.CullingResult => new CullingResultData(),
                ResourceType.TextureList => new TextureListData(),
                _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null)
            };
            name = ViewHelpers.MakeNameUnique(name, m_GraphView.currentGraph.ResourceList.Select(t => t.name).ToHashSet());
            resourceData.name = name;
            resourceData.category = blackboardCategory.title;
            m_GraphView.currentGraph.m_ResourceList.Add(resourceData);
            return new RPGBlackboardRow(resourceData);
        }
    }
}
