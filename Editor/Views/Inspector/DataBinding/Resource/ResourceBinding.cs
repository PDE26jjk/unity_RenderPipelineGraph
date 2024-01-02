using System;
using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Interface;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public partial class RPGBlackboardField : IRPGBindable {
        public ScriptableObject BindingObject() {
            return GetFirstAncestorOfType<RPGBlackboardRow>().BindingObject();
        }
    }
    public partial class RPGBlackboardRow : IRPGBindable {
        class ResourceDataBinding : BindingHelper<ResourceData> {
            public string name;
            bool inited = false;
            public virtual void Init(ResourceData model) {
                if(inited) return;
                inited = true;
                
                this.Model = model;
                name = Model.name;
            }
        }
        ResourceDataBinding m_BindingObject;
        public ScriptableObject BindingObject() {

            m_BindingObject ??= Model.type switch {
                ResourceType.Texture when Model is BuildInRenderTextureData => ScriptableObject.CreateInstance<BuildInTextureBinding>(),
                ResourceType.Texture => ScriptableObject.CreateInstance<TextureBinding>(),
                // ResourceType.Buffer => expr,
                // ResourceType.AccelerationStructure => expr,
                // ResourceType.RendererList => expr,
                // ResourceType.CullingResult => expr,
                // ResourceType.TextureList => expr,
                _ => ScriptableObject.CreateInstance<ResourceDataBinding>()
            };
            m_BindingObject.Init(this.Model);
            return m_BindingObject;
        }
        [CustomEditor(typeof(ResourceDataBinding))]
        public class ResourceDataBindingEditor : UnityEditor.Editor {
            public override VisualElement CreateInspectorGUI() {
                return new Label("UnKnown Resource");
            }
        }
    }
}
