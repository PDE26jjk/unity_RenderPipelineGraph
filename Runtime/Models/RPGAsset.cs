using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {


    public class RPGModel : ScriptableObject {
        internal RPGModel() {

        }
    }


    public class NodeData : RPGModel {
        public string exposedName;
        public Vector2 pos;
        public Color color;
    }

    [CreateAssetMenu(menuName = "Rendering/PRGAsset")]
    public class RPGAsset : ScriptableObject, ISerializationCallbackReceiver {
        int version = 0;
        [NonSerialized] public List<ResourceData> ResourceList;
        [NonSerialized] public List<NodeData> NodeList;
        [SerializeField] string content;
        public void OnBeforeSerialize() {
            // Debug.Log(JsonUtility.ToJson(ResourceList)); 
        }
        public void OnAfterDeserialize() {
        }
        public void TestInit() {
            ResourceList ??= new();
            NodeList ??= new();
            ResourceList.Clear();
            NodeList.Clear();

            var n1 = PassNodeData.Instance(typeof(TestPass1));
            var n2 = PassNodeData.Instance(typeof(TestPass1));
            var t1 = ScriptableObject.CreateInstance<TextureData>();
            t1.name = "ttt";
            var n3 = TextureNodeData.Instance(t1);
            
            n1.pos = new Vector2(100, 100);
            n3.pos = new Vector2(200, 100);
            n2.pos = new Vector2(300, 100);
            
            ResourceList.Add(t1);
            NodeList.Add(n1);
            NodeList.Add(n2);
            NodeList.Add(n3);

            PortData.Connect(n1.inputs["t1"],n3.attachTo);
            PortData.Connect(n3.attachTo,n2.inputs["t2"]);

        }
        private RPGAsset() {
        }
        public static bool Save() {
            return true;
        }
    }

}
