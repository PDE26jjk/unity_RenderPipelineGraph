using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public class RPGEditorBase : UnityEditor.Editor {
        static Dictionary<Tuple<Type, string>, FieldInfo> FieldCache = new();
        internal static FieldInfo GetField(Type type, string name) {
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
        internal VisualElement CreatePropertyField<T>(string path, object bindingData, string bindingPath = null, bool autoBinding = true, Action callBack = null) {
            bindingPath ??= path;
            SerializedProperty property = serializedObject.FindProperty(path);
            FieldInfo fieldInfo = null;
            autoBinding &= bindingData is not null;
            if (autoBinding) {
                fieldInfo = GetField(bindingData.GetType(), bindingPath);
            }
            VisualElement field = null;
            if (!typeof(T).IsEnum) {
                field = new PropertyField(property);
                field.RegisterCallback<ChangeEvent<T>>(env => {
                    if (env.newValue is null) return;
                    fieldInfo?.SetValue(bindingData, env.newValue);
                    callBack?.Invoke();
                });
            }
            else {
                // enum field will not invoke something like RegisterCallback<ChangeEvent<Enum>>, handle it separately.
                var enumField = new EnumField(ObjectNames.NicifyVariableName(path));
                field = enumField;
                enumField.AddToClassList("unity-base-field__aligned");
                enumField.BindProperty(property);
                enumField.RegisterValueChangedCallback(env => {
                    if (env.newValue is null)
                        return;
                    fieldInfo?.SetValue(bindingData, env.newValue);
                    callBack?.Invoke();
                });
            }
            return field;
        }

    }
}
