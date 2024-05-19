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
    public class PortData : JsonObject {
        [SerializeField]
        protected JsonRef<Slottable> m_Owner;
        public Slottable Owner => m_Owner.value;
        public PortType portType;
        [SerializeField]
        List<JsonRef<PortData>> m_LinkTo = new();
        public List<PortData> LinkTo => m_LinkTo.SelectValue().ToList();

        public List<Slottable> LinkToOwners => m_LinkTo.SelectValue().Select(t=>t.Owner).ToList();
        public override void OnAfterDeserialize() {
            base.OnAfterDeserialize();
            // foreach (JsonRef<PortData> linkTo in m_LinkTo) {
            //     if (linkTo.value == null) {
            //         m_LinkTo.Remove(linkTo);
            //     }
            // }
        }
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
        public void DisconnectAll() {
            foreach (var p in m_LinkTo.SelectValue()) {
                if (p != null && p.m_LinkTo.Contains(this)) {
                    p.m_LinkTo.Remove(this);
                }
            }
            m_LinkTo.Clear();
        }
        internal PortData(Slottable owner) {
            m_Owner = owner;
        }
        protected PortData(){}
    }
    public class ResourcePortData : PortData {
        public ResourceType resourceType;
        internal ResourcePortData(Slottable owner):base(owner) {
            this.portType = PortType.Resource;
        }
        
        ResourcePortData(){}


    }
    public sealed class DependencePortData : PortData {
        internal DependencePortData(Slottable owner):base(owner) {
            this.portType = PortType.Dependence;
        }
        
        DependencePortData(){}
    }

}
