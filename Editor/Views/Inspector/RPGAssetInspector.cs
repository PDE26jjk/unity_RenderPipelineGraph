using UnityEditor;
using UnityEngine;

namespace RenderPipelineGraph {
    [CustomEditor(typeof(RPGAsset)), CanEditMultipleObjects]
    public class RPGAssetInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var asset = this.target as RPGAsset;
            if (GUILayout.Button("edit")) {
                RPGWindow.ShowWindow(asset);
                // Undo.RecordObject(asset,"edit");
            }
        }
    }

}
