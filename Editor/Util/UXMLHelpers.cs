using UnityEditor;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor {
    static class UXMLHelpers {
        public static void AddStyleSheetPath(this VisualElement visualElement, string path) {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RenderPipelineGraph/Editor/UIResources/uss/" + path);
            if (sheet != null)
                visualElement.styleSheets.Add(sheet);
        }
        public static VisualElement LoadUXML(string path) {
            var visualTreeAsset = EditorGUIUtility.Load("Assets/RenderPipelineGraph/Editor/UIResources/uxml/" + path) as VisualTreeAsset;
            return visualTreeAsset ? visualTreeAsset.Instantiate() : null;
        }
    }
}
