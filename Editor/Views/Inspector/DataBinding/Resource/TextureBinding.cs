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
        public class TextureBinding : ResourceDataBinding {
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
                if (Model is TextureData textureData) {
                    loadDesc(textureData.m_desc);
                }
            }
            protected void loadDesc(RPGTextureDesc desc) {
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
        class TextureListBinding : TextureBinding {
            [FormerlySerializedAs("buffersCount")]
            public int bufferCount;
            public override void Init(ResourceData model) {
                base.Init(model);
                var textureListData = Model as TextureListData;
                bufferCount = textureListData.bufferCount;
                loadDesc(textureListData.m_desc);
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
                if (!CheckAndAddNameField(root, out TextureBinding textureBinding, out TextureData textureData)) {
                    return root;
                }
                root.Add(CreatePropertyField<RPGBuildInRTType>("buildInTextureType", textureData, "textureType"));
                return root;
            }
        }

        [CustomEditor(typeof(TextureBinding))]
        public class TextureBindingEditor : RPGEditorBase {
            public override VisualElement CreateInspectorGUI() {
                var root = new VisualElement();
                if (!CheckAndAddNameField(root, out TextureBinding textureBinding, out TextureData textureData)) {
                    return root;
                }
                var descData = textureData.m_desc.value;
                AddTextureCommonField(descData, textureBinding, root);

                return root;
            }
            protected void AddTextureCommonField(RPGTextureDesc descData, TextureBinding textureBinding, VisualElement root) {

                var size = CreatePropertyField<Vector2Int>("size", null, null, false, () => {
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
                var isDepth = CreatePropertyField<bool>("isDepth", null, callBack: () => {
                    depthBit.SetDisplay(textureBinding.isDepth || textureBinding.isShadowMap);
                    if (!textureBinding.isDepth && !textureBinding.isShadowMap) {
                        descData.depthBufferBits = DepthBits.None;
                    }
                });
                // var foldout = new Foldout();
                // foldout.text = "xxxx";
                // foldout.Add(new TextField("haha"));
                // root.Add(foldout);
                depthBit.SetDisplay(textureBinding.isDepth || textureBinding.isShadowMap);
                root.Add(isDepth);
                var isShadowMap = CreatePropertyField<bool>("isShadowMap", descData);
                root.Add(isShadowMap);
                var clearBuffer = CreatePropertyField<bool>("clearBuffer", descData);
                root.Add(depthBit);
                
                root.Add(clearBuffer);
                // TODO complete texture data binding
            }

        }
        [CustomEditor(typeof(TextureListBinding)), CanEditMultipleObjects]
        public class TextureListBindingEditor : TextureBindingEditor {
            public override VisualElement CreateInspectorGUI() {
                var root = new VisualElement();
                if (!CheckAndAddNameField(root, out TextureListBinding textureListBinding, out TextureListData textureListData)) {
                    return root;
                }
                var descData = textureListData.m_desc.value;
                var bufferCount = CreatePropertyField<int>("bufferCount", textureListData);
                root.Add(bufferCount);
                AddTextureCommonField(descData, textureListBinding, root);

                return root;
            }
        }
    }
}
