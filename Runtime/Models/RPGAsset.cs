using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public static List<RPGModel.RPGModelBinding> toInspectorBinding<T>(this List<T> models) where T : RPGModel {
            return models.Select(t => t.getInspectorBinding()).ToList();
        }
        public static RPGModel.RPGModelBinding[] toInspectorBinding<T>(this T[] models) where T : RPGModel {
            return models.Select(t => t.getInspectorBinding()).ToArray();
        }
    }

    public class NodeData : Slottable {
        public string exposedName;
        public Vector2 pos;
        public Color color;
    }

    [CreateAssetMenu(menuName = "Rendering/PRGAsset")]
    public class RPGAsset : ScriptableObject, ISerializationCallbackReceiver {

        [SerializeField] string content;
        public string Content => content;
        public bool Deserialized { get; private set; } = false;

        internal RPGGraphData m_Graph = new();

        internal bool NeedRecompile;

        public RPGGraphData Graph => m_Graph;

        public RPGGraphData Save() {
            m_Graph.TestInit3();
            printDebug(m_Graph);
            var json = MultiJson.Serialize(m_Graph);
            content = json;
            Debug.Log(json);
            var deserializedGraph = new RPGGraphData();
            MultiJson.Deserialize(deserializedGraph, json);
            printDebug(deserializedGraph);
            return deserializedGraph;
        }
        public void debug1() {
            Debug.Log(m_Graph);
        }
        public void printDebug(RPGGraphData graphData) {
            var str = new StringBuilder();
            foreach (NodeData nodeData in graphData.NodeList) {
                str.Append(nodeData.exposedName + ":" + nodeData.objectId + "\n");
                str.Append("param:\n");
                switch (nodeData) {
                    case PassNodeData passNodeData:
                        foreach (var parameter in passNodeData.Parameters.Values) {
                            switch (parameter) {
                                case CullingResultParameterData cullingResultParameterData:
                                    // str.Append(cullingResultParameterData)
                                    break;
                                case RendererListParameterData rendererListParameterData:
                                    str.Append(rendererListParameterData.cullingWhenEmpty);
                                    break;
                                case TextureListParameterData textureListParameter:
                                    str.Append(textureListParameter);
                                    break;
                                case TextureParameterData textureParameterData:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(parameter));

                            }
                            var portData = parameter.Port;
                            if (parameter.NeedPort() && portData is not null) {
                                var resourcePortData = (ResourcePortData)portData;
                                str.Append(resourcePortData.name + ":" + resourcePortData.objectId + "\n");
                                str.Append("linkTo:\n");
                                foreach (PortData data in resourcePortData.LinkTo) {
                                    str.Append(data.name + ":" + data.objectId + "\n");
                                }

                            }
                        }
                        break;
                    case TextureNodeData textureNodeData:
                        var at = textureNodeData.AttachTo;
                        str.Append(at.name + ":" + at.objectId + "\n");
                        str.Append("linkTo:\n");
                        foreach (PortData data in at.LinkTo) {
                            str.Append(data.name + ":" + data.objectId + "\n");
                        }
                        break;
                }
                str.Append("\n");
            }

            Debug.Log(str.ToString());
        }

        public void OnBeforeSerialize() {
            // Debug.Log("Asset Serialize");
        }
        public void OnAfterDeserialize() {
            // Debug.Log("Asset Deserialize");
        }
    }
}
