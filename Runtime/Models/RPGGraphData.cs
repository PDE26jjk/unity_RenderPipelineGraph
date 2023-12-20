using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public class RPGGraphData : RPGModel {
        [SerializeField]
        List<JsonData<ResourceData>> m_ResourceList = new();
        public List<ResourceData> ResourceList =>m_ResourceList.SelectValue().ToList();
        
        [SerializeField]
        List<JsonData<NodeData>> m_NodeList = new();
        public List<NodeData> NodeList => m_NodeList.SelectValue().ToList();

        public override void OnBeforeSerialize() {
            // Debug.Log(JsonUtility.ToJson(ResourceList)); 
            foreach (var data in m_ResourceList) {
                data.OnBeforeSerialize();
            }
            foreach (var data in m_NodeList) {
                data.OnBeforeSerialize();
            }
        }
        internal RPGGraphData(){}

        #region Test

        public void TestInit1() {
            m_ResourceList.Clear();
            m_NodeList.Clear();

            var pn1 = PassNodeData.Instance(typeof(TestPass1));
            var textureimport1 = new TextureData {
                name = "textureimport1",
                useType = UseType.Imported,
                rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<Texture>("Assets/RPG/input.png"))
            };

            var textureimport2 = new TextureData {
                name = "textureimport2",
                useType = UseType.Imported,
                rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RPG/output.renderTexture"))
            };

            var tn1 = TextureNodeData.Instance(textureimport1);
            var tn2 = TextureNodeData.Instance(textureimport2);

            pn1.pos = new Vector2(300, 100);
            tn1.pos = new Vector2(100, 100);
            tn2.pos = new Vector2(100, 200);

            m_ResourceList.Add(textureimport1);
            m_ResourceList.Add(textureimport2);
            m_NodeList.Add(pn1);
            m_NodeList.Add(tn1);
            m_NodeList.Add(tn2);

            PortData.Connect(pn1.Attachments["read1"], tn1.AttachTo);
            PortData.Connect(pn1.Attachments["write1"], tn2.AttachTo);

        }
        public void TestInit2() {
            m_ResourceList ??= new();
            m_NodeList ??= new();
            m_ResourceList.Clear();
            m_NodeList.Clear();

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
            var t1 = new TextureData();
            t1.name = "ttt";

            n2.pos = new Vector2(100, 100);
            n5.pos = new Vector2(100, 200);

            m_ResourceList.Add(t1);
            m_NodeList.Add(n1);
            m_NodeList.Add(n2);
            m_NodeList.Add(n3);
            m_NodeList.Add(n4);
            m_NodeList.Add(n5);
            m_NodeList.Add(n6);

            n2.dependencies.Add(n1);
            n3.dependencies.Add(n2);
            n4.dependencies.Add(n3);
            n5.dependencies.Add(n1);
            n6.dependencies.Add(n5);
            n4.dependencies.Add(n6);
        }
        public void TestInit3() {
            m_ResourceList.Clear();
            m_NodeList.Clear();

            var pn1 = PassNodeData.Instance(typeof(TestPass1));
            var pn2 = PassNodeData.Instance(typeof(TestPass1));
            var textureimport1 = new TextureData();
            textureimport1.name = "textureimport1";
            textureimport1.useType = UseType.Imported;
            textureimport1.rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<Texture>("Assets/RPG/input.png"));

            var textureimport2 = new TextureData();
            textureimport2.name = "textureimport2";
            textureimport2.useType = UseType.Imported;
            textureimport2.rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RPG/output.renderTexture"));

            var tn1 = TextureNodeData.Instance(textureimport1);
            var tn2 = TextureNodeData.Instance(textureimport2);

            pn1.pos = new Vector2(300, 100);
            pn2.pos = new Vector2(300, 300);
            tn1.pos = new Vector2(100, 100);
            tn2.pos = new Vector2(100, 200);

            m_ResourceList.Add(textureimport1);
            m_ResourceList.Add(textureimport2);
            m_NodeList.Add(pn1);
            m_NodeList.Add(pn2);
            m_NodeList.Add(tn1);
            m_NodeList.Add(tn2);

            pn2.dependencies.Add(pn1);

            PortData.Connect(pn1.Attachments["read1"], tn1.AttachTo);
            PortData.Connect(pn1.Attachments["write1"], tn2.AttachTo);

        }

        #endregion
    }

}
