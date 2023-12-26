using System;
using System.Collections.Generic;
using RenderPipelineGraph.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public class TextureListData : CanSetGlobalResourceData {
        
        public TextureListData() {
            type = ResourceType.TextureList;
            m_desc = new RPGTextureDesc();
        }
        public override void OnBeforeSerialize() {
            this.m_desc.OnBeforeSerialize();
            base.OnBeforeSerialize();
        }

        public bool isHistoryBuffer = false;

        public JsonData<RPGTextureDesc> m_desc;
        
        [NonSerialized]
        public List<TextureHandle> handle;
        
        [NonSerialized]
        public List<RTHandle> rtHandle;

    }

}
