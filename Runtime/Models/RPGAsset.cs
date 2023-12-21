using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using RenderPipelineGraph.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

// entry
namespace RenderPipelineGraph {

    public class RPGModel : JsonObject {
        internal RPGModel() {
            
        }
        public class RPGModelBinding : ScriptableObject {
            public int aaa = 1;
        }
        protected RPGModelBinding m_ObjBinding;
        public virtual RPGModelBinding getInspectorBinding() {
            m_ObjBinding ??= new();
            return m_ObjBinding;
        }

    }
    public static class RPGModelExtensions {
        public static List<RPGModel.RPGModelBinding> toMonoBehaviours<T>(this List<T> models) where T : RPGModel {
            return models.Select(t => t.getInspectorBinding()).ToList();
        }
        public static RPGModel.RPGModelBinding[] toMonoBehaviours<T>(this T[] models) where T : RPGModel {
            return models.Select(t => t.getInspectorBinding()).ToArray();
        }
    }

    public class NodeData : Slottable {
        public string exposedName;
        public Vector2 pos;
        public Color color;
    }

    [CreateAssetMenu(menuName = "Rendering/PRGAsset")]
    public class RPGAsset : ScriptableObject {

        [SerializeField] string content;
        public bool Deserialized { get; private set; } = false;

        // public for test
        public RPGGraphData m_Graph = new();

        public RPGGraphData Graph => m_Graph;

        public RPGGraphData Save() {
            m_Graph.TestInit3();
            printDebug(m_Graph);
            var json = MultiJson.Serialize(m_Graph);
            content = json;
            // Debug.Log(json);
            var deserializedGraph = new RPGGraphData();
            MultiJson.Deserialize(deserializedGraph, json);
            printDebug(deserializedGraph);
            return deserializedGraph;
        }
        public void debug1() {
            Debug.Log(m_Graph);
        }
        public void printDebug(RPGGraphData graphData) {
            var str = "";
            foreach (NodeData nodeData in graphData.NodeList) {
                str += (nodeData.exposedName + ":" + nodeData.objectId) + "\n";
                str += "port:\n";
                switch (nodeData) {
                    case PassNodeData passNodeData:
                        foreach (var portData in passNodeData.Parameters.Values.Select(t=>t.Port)) {
                            var resourcePortData = (ResourcePortData)portData;
                            str += resourcePortData.name + ":" + resourcePortData.objectId + "\n";
                            str += "linkTo:\n";
                            foreach (PortData data in resourcePortData.LinkTo) {
                                str += data.name + ":" + data.objectId + "\n";
                            }
                        }
                        break;
                    case TextureNodeData textureNodeData:
                        var at = textureNodeData.AttachTo;
                        str += at.name + ":" + at.objectId + "\n";
                        str += "linkTo:\n";
                        foreach (PortData data in at.LinkTo) {
                            str += data.name + ":" + data.objectId + "\n";
                        }
                        break;
                }
                str += "\n";
            }

            Debug.Log(str);
        }
    }
}
