using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor {
    static class UXMLHelpers {
        public static void AddStyleSheetPath(this VisualElement visualElement, string path) {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(PackageResourcePath + path);
            if (sheet != null)
                visualElement.styleSheets.Add(sheet);
        }
        public static VisualElement LoadUXML(string path) {
            var visualTreeAsset = EditorGUIUtility.Load("Assets/RenderPipelineGraph/Editor/UIResources/uxml/" + path) as VisualTreeAsset;
            return visualTreeAsset ? visualTreeAsset.Instantiate() : null;
        }
        public static string PackageResourcePath => "Assets/RenderPipelineGraph/Editor/Resources/";

        public static StyleSheet LoadStyleSheet(string path) {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(PackageResourcePath + path);
        }
    }
}
