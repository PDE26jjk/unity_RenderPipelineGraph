using System;
using System.Collections.Generic;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {

    public class RPGCullingDesc : JsonObject {
        public string cullingFunctionTypeName = typeof(CullingDefault).FullName;

        public override bool Equals(object obj) {
            if (obj is RPGCullingDesc)
                return Equals((RPGCullingDesc)obj);
            return  false;
        }
        public override int GetHashCode() {
            return (cullingFunctionTypeName != null ? cullingFunctionTypeName.GetHashCode() : 0);
        }
        protected bool Equals(RPGCullingDesc obj) {
            return obj.cullingFunctionTypeName == cullingFunctionTypeName;
        }
    }

    public class CullingResultData : ResourceData {
        
        public JsonData<RPGCullingDesc> m_CullingDesc;
        
        [NonReorderable]
        public CullingDefault cullingFunc = null;
        
        [NonReorderable]
        public CullingResults cullingResults;

        public CullingResultData() {
            m_CullingDesc = new RPGCullingDesc();
            this.type = ResourceType.CullingResult;
            usage = Usage.Created;
        }

    }
}
