﻿using System.Collections.Generic;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {
    public class TestHistoryRT : RPGPass {
        public class PassData {
            [ListSize(2),Read(new []{0,1}),Fragment(0,0)]
            public List<TextureHandle> read1;
        }
        public override bool Valid(CameraData cameraData) {
            return true;
        }

        public TestHistoryRT():base() {
        }
        
        public static void Record(PassData passData, RasterGraphContext context) {
            var cmd = context.cmd;
        }
    }
}
