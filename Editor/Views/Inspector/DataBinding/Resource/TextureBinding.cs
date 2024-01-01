using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph.Interface;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public partial class RPGBlackboardRow : IRPGBindable {
        class TextureBinding : ResourceDataBinding {
            public string textureName;
            ///<summary>Texture sizing mode.</summary>
            public TextureSizeMode sizeMode;
            public int width;
            public int height;
            public int slices;
            public Vector2 scale;
            public GraphicsFormat colorFormat;
            public DepthBits depthBufferBits;
            public FilterMode filterMode;
            public TextureDimension dimension;
            public bool clearBuffer;
            public Color clearColor;
            public bool enableRandomWrite;
            public bool isShadowMap;
            public MSAASamples msaaSamples;

            public override void Init(ResourceData model) {
                base.Init(model);
                var textureData = Model as TextureData;
                var desc = textureData.m_desc.value;
                width = desc.width;
                height = desc.height;
                slices = desc.slices;
                scale = desc.scale;
                colorFormat = desc.colorFormat;
                depthBufferBits = desc.depthBufferBits;
                filterMode = desc.filterMode;
                dimension = desc.dimension;
                clearBuffer = desc.clearBuffer;
                clearColor = desc.clearColor;
                enableRandomWrite = desc.enableRandomWrite;
                isShadowMap = desc.isShadowMap;
                msaaSamples = desc.msaaSamples;
            }
        }

        [CustomEditor(typeof(TextureBinding)), CanEditMultipleObjects]
        public class TextureBindingBindingEditor : UnityEditor.Editor {
            static Dictionary<Tuple<Type, string>, FieldInfo> FieldCache = new();
            static FieldInfo GetField(Type type, string name) {
                var tuple = new Tuple<Type, string>(type, name);
                if (!FieldCache.TryGetValue(tuple, out var fieldInfo)) {
                    FieldInfo[] fieldInfos = type.GetFields();
                    FieldCache[tuple] = fieldInfo = fieldInfos.First(t => t.Name == name);
                }
                if (fieldInfo is null) {
                    throw new Exception($"{type.FullName} has not public field named {name}");
                }
                return fieldInfo;
            }
            public VisualElement CreatePropertyField<T>(string path, object bindingData, string bindingPath = null) {
                bindingPath ??= path;
                SerializedProperty property = serializedObject.FindProperty(path);
                FieldInfo fieldInfo = GetField(bindingData.GetType(), bindingPath);
                VisualElement field = null;
                if (!typeof(T).IsEnum) {
                    field = new PropertyField(property);
                    field.RegisterCallback<ChangeEvent<T>>(env => {
                        if (env.newValue is null) return;
                        fieldInfo.SetValue(bindingData, env.newValue);
                        //
                    });
                }
                else {
                    var enumField = new EnumField(ObjectNames.NicifyVariableName(path));
                    field = enumField;
                    enumField.AddToClassList("unity-base-field__aligned");
                    enumField.BindProperty(property);
                    enumField.RegisterValueChangedCallback(env => {
                        if (env.newValue is null)
                            return;
                        fieldInfo.SetValue(bindingData, env.newValue);
                        //
                    });
                }
                return field;
            }
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
                var height = CreatePropertyField<int>("height", textureData.m_desc.value);
                root.Add(height);
                var colorFormat = CreatePropertyField<GraphicsFormat>("colorFormat", textureData.m_desc.value);
                root.Add(colorFormat);
                return root;
            }


        }
    }
}
