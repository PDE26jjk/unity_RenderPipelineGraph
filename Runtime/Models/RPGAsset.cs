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
        RPGModelBinding m_Obj;
        public virtual RPGModelBinding getInspectorBinding() {
            m_Obj ??= new();
            return m_Obj;
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

    public class NodeData : RPGModel {
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
            var json = MultiJson.Serialize(m_Graph);
            content = json;
            Debug.Log(json);
            var deserializedGraph = new RPGGraphData();
            MultiJson.Deserialize(deserializedGraph, json);
            return deserializedGraph;
        }

    }

}
