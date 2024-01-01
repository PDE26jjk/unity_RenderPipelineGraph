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
        public static bool GetValidViewModel(VisualElement visualElement, out ResourceViewModel resourceViewModel) {
            RPGView graphView = visualElement.GetFirstAncestorOfType<RPGView>();
            resourceViewModel = graphView.m_ResourceViewModel;
            return !graphView.m_NodeViewModel.Loading;
        }
        Dictionary<ResourceData, RPGBlackboardRow> m_Resources = new();
        internal bool TryGetResourceViews(ResourceData data, out RPGBlackboardRow row) => m_Resources.TryGetValue(data, out row);
        internal RPGView m_GraphView;
        public ResourceViewModel(RPGView graphView) {
            this.m_GraphView = graphView;
        }
        // blackboard use it to load resources.
        public IEnumerable<Tuple<string, RPGBlackboardRow>> LoadResources() {
            Dictionary<ResourceData, RPGBlackboardRow> newResources = new();
            foreach (ResourceData resourceData in m_GraphView.currentGraph.ResourceList) {
                if (!TryGetResourceViews(resourceData, out var row))
                    row = new RPGBlackboardRow(resourceData);
                newResources[resourceData] = row;
                yield return new Tuple<string, RPGBlackboardRow>(resourceData.category, row);
            }
            m_Resources.Clear();
            m_Resources = newResources;
        }
        internal bool needUpdateDefaultResourceList = true;
        List<RPGBlackboardRow>[] DefaultResourceLists = new List<RPGBlackboardRow>[(int)ResourceType.Count];
        List<string>[] DefaultResourceStrLists = new List<string>[(int)ResourceType.Count];
        public List<string> GetDefaultResourceNameList(ResourceType resourceType) {
            if (needUpdateDefaultResourceList) {
                needUpdateDefaultResourceList = false;
                var defaultResourceDatas = m_Resources.Keys.Where(t => t.isDefault);
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
            needUpdateDefaultResourceList = true;
            return new RPGBlackboardRow(resourceData);
        }
    }
    static class ResourceRowExtension {
        public static void NotifyResourceDeleteVM(this RPGBlackboardRow row) {
            if (!ResourceViewModel.GetValidViewModel(row, out var resourceViewModel)) return;
            if (resourceViewModel.m_GraphView.currentGraph.m_ResourceList.Remove(row.Model)) {
                resourceViewModel.needUpdateDefaultResourceList = true;
            }

        }
        public static void NotifyResourceChangeVM(this RPGBlackboardRow row) {
            if (!ResourceViewModel.GetValidViewModel(row, out var resourceViewModel)) return;
            // resourceViewModel.m_GraphView.currentGraph.ResourceList.Remove(row.model);
            resourceViewModel.needUpdateDefaultResourceList = true;
        }
    }
}
