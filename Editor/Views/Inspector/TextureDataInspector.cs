using UnityEditor;
using UnityEngine;

namespace RenderPipelineGraph.Editor.Views.Inspector {
        [CustomEditor(typeof(TextureNodeData))]
        public class TextureDataInspector : UnityEditor.Editor {
            public override void OnInspectorGUI() {
                var asset = this.target as RPGAsset;
                GUILayout.Label("fefef");
                if (GUILayout.Toggle(true,"aa")) {
                    
                }
            }
        }

}
