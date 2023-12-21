using System;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public abstract class RPGParameterData : Slottable {
        // public abstract T Value();
        public override void OnBeforeSerialize() {
            m_Port.OnBeforeSerialize();
        }
        protected ResourceData m_DefaultResource;
        public void SetDefaultResource(ResourceData resourceData) {
            m_DefaultResource = resourceData;
        }
        public virtual object GetValue() {
            if (UseDefault) return m_DefaultResource;
            if (Port.LinkTo.Count > 0) {
                return (Port.LinkTo[0].Owner as ResourceNodeData)?.Resource;
            }
            return null;
        }
        [SerializeField]
        string m_Name;
        public string Name {
            get => m_Name;
            set => m_Name = value;
        }
        [SerializeField]
        protected JsonData<PortData> m_Port;
        public PortData Port =>
            m_Port;
        public bool UseDefault  = false;
        public virtual bool NeedPort() {
            return true;
        }
        public virtual bool CanConvertTo(Type type) {
            return false;
        }
    }
}
