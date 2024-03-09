using System;
using System.Linq;
using RenderPipelineGraph.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public partial class RPGBlackboardRow : IRPGBindable {
        class TextureBinding : ResourceDataBinding {
            public string textureName;
            ///<summary>Texture sizing mode.</summary>
            public TextureSizeMode sizeMode;
            public int slices;
            public Vector2 scale;
            public Vector2Int size;
            public GraphicsFormat colorFormat;
            public DepthBits depthBufferBits;
            public FilterMode filterMode;
            public TextureDimension dimension;
            public bool clearBuffer;
            public Color clearColor;
            public bool enableRandomWrite;
            public bool isShadowMap;
            public bool isDepth;
            public MSAASamples msaaSamples;

            public override void Init(ResourceData model) {
                base.Init(model);
                var textureData = Model as TextureData;
                var desc = textureData.m_desc.value;
                size = new Vector2Int(desc.width, desc.height);
                sizeMode = desc.sizeMode;
                slices = desc.slices;
                scale = desc.scale;
                colorFormat = desc.colorFormat;
                depthBufferBits = desc.depthBufferBits;
                filterMode = desc.filterMode;
                dimension = desc.dimension;
                clearBuffer = desc.clearBuffer;
                clearColor = desc.clearColor;
                isDepth = desc.depthBufferBits != DepthBits.None;
                enableRandomWrite = desc.enableRandomWrite;
                isShadowMap = desc.isShadowMap;
                msaaSamples = desc.msaaSamples;
            }
        }
        class BuildInTextureBinding : TextureBinding {
            public RPGBuildInRTType buildInTextureType;
            public override void Init(ResourceData model) {
                base.Init(model);
                var textureData = Model as BuildInRenderTextureData;
                buildInTextureType = textureData.textureType;
            }
        }
        [CustomEditor(typeof(BuildInTextureBinding)), CanEditMultipleObjects]
        public class BuildInTextureBindingEditor : RPGEditorBase {
            public override VisualElement CreateInspectorGUI() {
                var root = new VisualElement();
                var textureBindings = serializedObject.targetObjects.Cast<TextureBinding>().ToList();
                var textureDatas = textureBindings.Select(t => t.Model).Cast<TextureData>().ToList();
                var textureData = textureDatas[0];
                if (textureData is null) return null;
                var nameField = CreatePropertyField<string>("name", textureData);
                if (textureBindings.Count == 1) root.Add(nameField);
                else
                    root.Add(new TextField("name") {
                        value = "---",
                        enabledSelf = false
                    });
                root.Add(CreatePropertyField<RPGBuildInRTType>("buildInTextureType", textureData,"textureType"));
                return root;
            }
        }

        [CustomEditor(typeof(TextureBinding)), CanEditMultipleObjects]
        public class TextureBindingEditor : RPGEditorBase {
            public override VisualElement CreateInspectorGUI() {
                var root = new VisualElement();
                var textureBindings = serializedObject.targetObjects.Cast<TextureBinding>().ToList();
                var textureDatas = textureBindings.Select(t => t.Model).Cast<TextureData>().ToList();
                var textureData = textureDatas[0];
                var textureBinding = textureBindings[0];
                if (textureData is null) return null;
                var nameField = CreatePropertyField<string>("name", textureData);
                if (textureBindings.Count == 1) root.Add(nameField);
                else
                    root.Add(new TextField("name") {
                        value = "---",
                        enabledSelf = false
                    });
                var descData = textureData.m_desc.value;
                var size = CreatePropertyField<Vector2Int>("size", null,null,false, () => {
                    descData.width = textureBinding.size.x;
                    descData.height = textureBinding.size.y;
                });
                var scale = CreatePropertyField<Vector2>("scale", descData);
                var colorFormat = CreatePropertyField<GraphicsFormat>("colorFormat", descData);
                root.Add(colorFormat);
                Action sizeModeChange = () => {
                    size.SetDisplay(descData.sizeMode == TextureSizeMode.Explicit);
                    scale.SetDisplay(descData.sizeMode == TextureSizeMode.Scale);
                };
                sizeModeChange.Invoke();
                var sizeMode = CreatePropertyField<TextureSizeMode>("sizeMode", descData, null, true, sizeModeChange);
                root.Add(sizeMode);
                root.Add(scale);
                root.Add(size);
                var depthBit = CreatePropertyField<DepthBits>("depthBufferBits", descData);
                var isDepth = CreatePropertyField<bool>("isDepth", null,callBack: () => {
                    depthBit.SetDisplay(textureBinding.isDepth);
                });
                // var foldout = new Foldout();
                // foldout.text = "xxxx";
                // foldout.Add(new TextField("haha"));
                // root.Add(foldout);
                depthBit.SetDisplay(textureBinding.isDepth);
                root.Add(isDepth);
                root.Add(depthBit);

                // TODO complete texture data binding
                
                return root;
            }

        }
    }
}
