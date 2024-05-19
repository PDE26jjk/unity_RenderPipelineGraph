using System;
using System.Linq;
using RenderPipelineGraph.Editor;
using RenderPipelineGraph.Editor.Views.blackborad;
using RenderPipelineGraph.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RenderPipelineGraph {
    public partial class PassNodeView : IRPGBindable {
        public class PassDataBinding : BindingHelper<PassNodeData> {
            public string name;
            bool inited = false;
            public virtual void Init(PassNodeData model) {
                if (inited) return;
                inited = true;

                this.Model = model;
                name = Model.exposedName;
            }
        }
        PassDataBinding m_BindingObject;
        PlaceholderObject m_PlaceholderObject;// multiple select must be same type, or Unity could crash.
        public Object BindingObject(bool multiple=false) {
            if (multiple) {
                m_PlaceholderObject ??= ScriptableObject.CreateInstance<PlaceholderObject>();
                return m_PlaceholderObject;
            }
            m_BindingObject ??= new PassDataBinding();
            m_BindingObject.Init(this.Model);
            return m_BindingObject;
        }
    }
    [CustomEditor(typeof(PassNodeView.PassDataBinding))]
    public class PassDataBindingEditor : RPGEditorBase {
        // Copy from SRP RenderGraphViewer 
        static string ScriptAbsolutePathToRelative(string absolutePath) {
            // Get path relative to project root and canonize directory separators
            var relativePath = FileUtil.GetLogicalPath(absolutePath);

            // Project assets start with data path
            if (relativePath.StartsWith(Application.dataPath, StringComparison.OrdinalIgnoreCase))
                relativePath = relativePath.Replace(Application.dataPath, "Assets");

            // Remove starting "./" if present, it breaks LoadAssetAtPath
            if (relativePath.StartsWith("./"))
                relativePath = relativePath.Substring(2);

            // Package cache path doesn't work with LoadAssetAtPath, so convert it to a Packages path
            if (relativePath.StartsWith("Library/PackageCache/"))
                relativePath = relativePath.Replace("Library/PackageCache/", "Packages/");

            return relativePath;
        }

        UnityEngine.Object FindScriptAssetByAbsolutePath(string absolutePath) {
            var relativePath = ScriptAbsolutePathToRelative(absolutePath);
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath);
            if (asset == null)
                Debug.LogWarning($"Could not find a script asset to open for file path {absolutePath}");
            return asset;
        }

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();
            var dataBindings = serializedObject.targetObjects.Cast<PassNodeView.PassDataBinding>().ToList();
            var dataBinding = dataBindings[0];
            var nameLabel = new Label(dataBinding.name);
            root.Add(nameLabel);
            var editButton = new Button() {
                text = "Edit"
            };
            editButton.clicked += (() => {
                string passFilePath = dataBinding.Model.Pass.filePath;
                if (passFilePath != "") {
                    var scriptAsset = FindScriptAssetByAbsolutePath(passFilePath);
                    AssetDatabase.OpenAsset(scriptAsset, 0);
                }
            });
            root.Add(editButton);
            return root;
        }
    }
}
