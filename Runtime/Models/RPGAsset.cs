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

        internal virtual string SaveToJson() {
            return "{}";
        }
        internal virtual void LoadFromJson(string json) {
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
        [NonSerialized] public List<ResourceData> ResourceList = new();
        [NonSerialized] public List<NodeData> NodeList = new();
        [SerializeField] string content;
        public void OnBeforeSerialize() {
            // Debug.Log(JsonUtility.ToJson(ResourceList)); 
        }
        public void OnAfterDeserialize() {
        }

        #region Test

        public void TestInit1() {
            ResourceList.Clear();
            NodeList.Clear();

            var pn1 = PassNodeData.Instance(typeof(TestPass1));
            var textureimport1 = ScriptableObject.CreateInstance<TextureData>();
            textureimport1.name = "textureimport1";
            textureimport1.useType = UseType.Imported;
            textureimport1.rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<Texture>("Assets/RPG/input.png"));

            var textureimport2 = ScriptableObject.CreateInstance<TextureData>();
            textureimport2.name = "textureimport2";
            textureimport2.useType = UseType.Imported;
            textureimport2.rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RPG/output.renderTexture"));

            var tn1 = TextureNodeData.Instance(textureimport1);
            var tn2 = TextureNodeData.Instance(textureimport2);

            pn1.pos = new Vector2(300, 100);
            tn1.pos = new Vector2(100, 100);
            tn2.pos = new Vector2(100, 200);

            ResourceList.Add(textureimport1);
            ResourceList.Add(textureimport2);
            NodeList.Add(pn1);
            NodeList.Add(tn1);
            NodeList.Add(tn2);

            PortData.Connect(pn1.Attachments["read1"], tn1.attachTo);
            PortData.Connect(pn1.Attachments["write1"], tn2.attachTo);

        }
        public void TestInit2() {
            ResourceList ??= new();
            NodeList ??= new();
            ResourceList.Clear();
            NodeList.Clear();

            var n1 = PassNodeData.Instance(typeof(TestPass1));
            var n2 = PassNodeData.Instance(typeof(TestPass1));
            var n3 = PassNodeData.Instance(typeof(TestPass1));
            var n4 = PassNodeData.Instance(typeof(TestPass1));
            var n5 = PassNodeData.Instance(typeof(TestPass1));
            var n6 = PassNodeData.Instance(typeof(TestPass1));
            n1.exposedName = "1";
            n2.exposedName = "2";
            n3.exposedName = "3";
            n4.exposedName = "4";
            n5.exposedName = "5";
            n6.exposedName = "6";
            var t1 = ScriptableObject.CreateInstance<TextureData>();
            t1.name = "ttt";

            n2.pos = new Vector2(100, 100);
            n5.pos = new Vector2(100, 200);

            ResourceList.Add(t1);
            NodeList.Add(n1);
            NodeList.Add(n2);
            NodeList.Add(n3);
            NodeList.Add(n4);
            NodeList.Add(n5);
            NodeList.Add(n6);

            n2.dependencies.Add(n1);
            n3.dependencies.Add(n2);
            n4.dependencies.Add(n3);
            n5.dependencies.Add(n1);
            n6.dependencies.Add(n5);
            n4.dependencies.Add(n6);
        }
        public void TestInit3() {
            ResourceList.Clear();
            NodeList.Clear();

            var pn1 = PassNodeData.Instance(typeof(TestPass1));
            var pn2 = PassNodeData.Instance(typeof(TestPass1));
            var textureimport1 = ScriptableObject.CreateInstance<TextureData>();
            textureimport1.name = "textureimport1";
            textureimport1.useType = UseType.Imported;
            textureimport1.rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<Texture>("Assets/RPG/input.png"));

            var textureimport2 = ScriptableObject.CreateInstance<TextureData>();
            textureimport2.name = "textureimport2";
            textureimport2.useType = UseType.Imported;
            textureimport2.rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RPG/output.renderTexture"));

            var tn1 = TextureNodeData.Instance(textureimport1);
            var tn2 = TextureNodeData.Instance(textureimport2);

            pn1.pos = new Vector2(300, 100);
            pn2.pos = new Vector2(300, 300);
            tn1.pos = new Vector2(100, 100);
            tn2.pos = new Vector2(100, 200);

            ResourceList.Add(textureimport1);
            ResourceList.Add(textureimport2);
            NodeList.Add(pn1);
            NodeList.Add(pn2);
            NodeList.Add(tn1);
            NodeList.Add(tn2);

            pn2.dependencies.Add(pn1);

            PortData.Connect(pn1.Attachments["read1"], tn1.attachTo);
            PortData.Connect(pn1.Attachments["write1"], tn2.attachTo);

        }

        #endregion

        private RPGAsset() {
        }
        public bool Save() {
            foreach (ResourceData resourceData in ResourceList) {

            }
            return true;
        }
    }

}
