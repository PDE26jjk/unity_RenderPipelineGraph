using System.Collections.Generic;
using System.Linq;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public enum PortType {
        Resource,
        Dependence
    }
    public class PortData : RPGModel {
        [SerializeField]
        protected JsonRef<NodeData> m_Owner;
        public NodeData Owner => m_Owner.value;
        public PortType portType;
        [SerializeField]
        List<JsonRef<PortData>> m_LinkTo = new();
        public List<PortData> LinkTo => m_LinkTo.SelectValue().ToList();

        public string name;
        public static void Connect(PortData p1, PortData p2) {
            if (!p1.m_LinkTo.Contains(p2)) {
                p1.m_LinkTo.Add(p2);
            }
            if (!p2.m_LinkTo.Contains(p1)) {
                p2.m_LinkTo.Add(p1);
            }
        }
        public static bool Disconnect(PortData p1, PortData p2) {
            if (p1.m_LinkTo.Contains(p2)) {
                p1.m_LinkTo.Remove(p2);
            }
            if (p2.m_LinkTo.Contains(p1)) {
                p2.m_LinkTo.Remove(p1);
            }
            return true;
        }
    }
    public class ResourcePortData : PortData {
        public ResourceType resourceType;
        public ResourcePortData(NodeData owner) {
            this.portType = PortType.Resource;
            this.m_Owner = owner;
        }
        internal ResourcePortData() {
        }
    }
    public class DependencePortData : PortData {
        public DependencePortData(NodeData owner) {
            this.portType = PortType.Dependence;
            this.m_Owner = owner;
        }
    }

}
