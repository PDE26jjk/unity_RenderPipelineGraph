using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine.Rendering;
namespace RenderPipelineGraph.Editor.ShaderGraph {
    
    static class CreateNewShaderGraph {
        [MenuItem("Assets/Create/Shader Graph/RPG/New Shader Graph", priority = CoreUtils.Priorities.assetsCreateShaderMenuPriority)]
        public static void CreateShaderGraph() {

            // var blockDescriptors = new[]
            // {
            //     BlockFields.VertexDescription.Position,
            //     BlockFields.VertexDescription.Normal,
            //     BlockFields.VertexDescription.Tangent,
            //     BlockFields.SurfaceDescription.BaseColor,
            //     BlockFields.SurfaceDescription.NormalTS,
            //     BlockFields.SurfaceDescription.Metallic,
            //     BlockFields.SurfaceDescription.Smoothness,
            //     BlockFields.SurfaceDescription.Emission,
            //     BlockFields.SurfaceDescription.Occlusion,
            // };
            Assembly assembly = typeof(UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInLitGUI).Assembly;
            Type buildInTargetType = assembly.GetType("UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInTarget");
            object target = Activator.CreateInstance(buildInTargetType);
            Type GraphUtilType = assembly.GetType("UnityEditor.ShaderGraph.GraphUtil");
            Type BlockFieldDescriptorType = assembly.GetType("UnityEditor.ShaderGraph.BlockFieldDescriptor");
            Array targets = Array.CreateInstance(buildInTargetType, 1);
            targets.SetValue(target,0);
            Array blocks = Array.CreateInstance(BlockFieldDescriptorType, 0);
            GraphUtilType.GetMethod("CreateNewGraphWithOutputs").Invoke(null, new [] { targets, blocks });
            
            
        }
    }
}
