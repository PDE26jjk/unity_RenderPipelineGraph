using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RenderPipelineGraph.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace RenderPipelineGraph {
    public class RPGGraphData : RPGModel {
        [SerializeField]
        internal List<JsonData<ResourceData>> m_ResourceList = new();
        public List<ResourceData> ResourceList => m_ResourceList.SelectValue().ToList();

        [SerializeField]
        internal List<JsonData<NodeData>> m_NodeList = new();
        public List<NodeData> NodeList => m_NodeList.SelectValue().ToList();
        [SerializeField]
        internal List<string> categorys = new();

        public override void OnBeforeSerialize() {
            // Debug.Log(JsonUtility.ToJson(ResourceList)); 
            foreach (var data in m_ResourceList) {
                data.OnBeforeSerialize();
            }
            foreach (var data in m_NodeList) {
                data.OnBeforeSerialize();
            }
        }
        internal RPGGraphData() {
        }

        #region Test

        public void TestInit1() {
            m_ResourceList.Clear();
            m_NodeList.Clear();

            var pn1 = PassNodeData.Instance(typeof(TestPass1));
            var textureimport1 = new TextureData {
                name = "textureimport1",
                usage = Usage.Imported,
                rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<Texture>("Assets/RPG/input.png"))
            };

            var textureimport2 = new TextureData {
                name = "textureimport2",
                usage = Usage.Imported,
                rtHandle = RTHandles.Alloc(AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RPG/output.renderTexture"))
            };

            var rendererListData = new RendererListData() {
                name = "rendererList1"
            };

            var tn1 = TextureNodeData.Instance(textureimport1);
            var tn2 = TextureNodeData.Instance(textureimport2);

            pn1.pos = new Vector2(300, 100);
            tn1.pos = new Vector2(100, 100);
            tn2.pos = new Vector2(100, 200);

            m_ResourceList.Add(textureimport1);
            m_ResourceList.Add(textureimport2);
            m_ResourceList.Add(rendererListData);
            m_NodeList.Add(pn1);
            m_NodeList.Add(tn1);
            m_NodeList.Add(tn2);

            PortData.Connect(pn1.Parameters["read1"].Port, tn1.AttachTo);
            PortData.Connect(pn1.Parameters["write1"].Port, tn2.AttachTo);

        }
        public void TestInit2() {
            m_ResourceList.Clear();
            m_NodeList.Clear();

            var n1 = PassNodeData.Instance(typeof(TestPass1));
            var n2 = PassNodeData.Instance(typeof(TestPass1));
            var n3 = PassNodeData.Instance(typeof(TestPass1));
            var n4 = PassNodeData.Instance(typeof(TestPass1));
            var n5 = PassNodeData.Instance(typeof(TestPass1));
            var n6 = PassNodeData.Instance(typeof(TestPass1));
            n1.exposedName = "1";
            n2.exposedName = "2";
            n3.exposedName = "3";
            n4.exposedName = "4";
            n5.exposedName = "5";
            n6.exposedName = "6";
            var t1 = new TextureData();
            t1.name = "ttt";

            n2.pos = new Vector2(100, 100);
            n5.pos = new Vector2(100, 200);

            m_ResourceList.Add(t1);
            m_NodeList.Add(n1);
            m_NodeList.Add(n2);
            m_NodeList.Add(n3);
            m_NodeList.Add(n4);
            m_NodeList.Add(n5);
            m_NodeList.Add(n6);

            n2.dependencies.Add(n1);
            n3.dependencies.Add(n2);
            n4.dependencies.Add(n3);
            n5.dependencies.Add(n1);
            n6.dependencies.Add(n5);
            n4.dependencies.Add(n6);
        }
        public void TestInit3() {
            m_ResourceList.Clear();
            m_NodeList.Clear();
            categorys.Clear();
            categorys.Add("Default");
            categorys.Add(string.Empty);

            var setupGlobalPass = PassNodeData.Instance(typeof(SetupGlobalConstantPass));
            var setupLightPass = PassNodeData.Instance(typeof(SetupLightShadow));
            var dirLightPass = PassNodeData.Instance(typeof(DirectionLightShadowMap));
            var otherLightPass = PassNodeData.Instance(typeof(OtherLightShadowMap));
            var unlitPass = PassNodeData.Instance(typeof(TestPassUnlit));
            var pn2 = PassNodeData.Instance(typeof(TestFinalBlitPass));
            var colorRT = new TextureData {
                category = "Default",
                name = "defaultColor",
                usage = Usage.Created,
                m_desc = new RPGTextureDesc {
                    sizeMode = TextureSizeMode.Scale,
                    scale = Vector2.one,
                    depthBufferBits = DepthBits.None,
                    filterMode = FilterMode.Bilinear,
                    name = "colorAttachment",
                    colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
                    clearBuffer = true,
                    clearColor = Color.clear,
                    enableRandomWrite = true
                }
            };
            var depthRT = new TextureData {
                category = "Default",
                name = "defaultDepth",
                usage = Usage.Created,
                m_desc = new RPGTextureDesc {
                    sizeMode = TextureSizeMode.Scale,
                    scale = Vector2.one,
                    depthBufferBits = DepthBits.Depth32,
                    filterMode = FilterMode.Bilinear,
                    name = "depthAttachment",
                    clearBuffer = true,
                    clearColor = new Color(0.5f, 0, 0)
                }
            };
            
            var dirShadowMapRT = new TextureData {
                category = "Default",
                name = "dirShadowMapRT",
                usage = Usage.Created,
                m_desc = new RPGTextureDesc {
                    sizeMode = TextureSizeMode.Explicit,
                    width = 1024,
                    height = 1024,
                    colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
                    depthBufferBits = DepthBits.Depth32,
                    filterMode = FilterMode.Bilinear,
                    name = "dirShadowMapRT",
                    clearBuffer = true,
                    clearColor = new Color(0.0f, 0, 0),
                    isShadowMap = true
                },
                SetGlobalTextureAfterAfterWritten = true,
                ShaderPropertyIdStr = "_DirectionalShadowAtlas"
            };
            
            var otherShadowMapRT = new TextureData {
                category = "Default",
                name = "otherShadowMapRT",
                usage = Usage.Created,
                m_desc = new RPGTextureDesc {
                    sizeMode = TextureSizeMode.Explicit,
                    width = 1024,
                    height = 1024,
                    colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
                    depthBufferBits = DepthBits.Depth32,
                    filterMode = FilterMode.Bilinear,
                    name = "otherShadowMapRT",
                    clearBuffer = true,
                    clearColor = new Color(0.0f, 0, 0),
                    isShadowMap = true
                },
                SetGlobalTextureAfterAfterWritten = true,
                ShaderPropertyIdStr = "_OtherShadowAtlas"
            };

            var targetRT = BuildInRenderTextureData.GetTexture(RPGBuildInRTType.CameraTarget);

            var tn1 = new ResourceNodeData();
            var rendererListData = new RendererListData();
            rendererListData.name = "rendererList1";
            RPGRenderListDesc rpgRenderListDesc = rendererListData.m_RenderListDesc.value;
            rpgRenderListDesc.shaderTagIdStrs.Add("MySRPMode1");
            tn1.SetResource(rendererListData);

            unlitPass.pos = new Vector2(300, 100);
            tn1.pos = new Vector2(100, 100);
            
            
            var cullingResultData = new CullingResultData(){name = "cullingResult1"};
            
            dirLightPass.dependencies.Add(setupLightPass);
            otherLightPass.dependencies.Add(setupLightPass);
            setupGlobalPass.dependencies.Add(dirLightPass);
            setupGlobalPass.dependencies.Add(otherLightPass);
            unlitPass.dependencies.Add(setupGlobalPass);
            unlitPass.dependencies.Add(setupGlobalPass); 
            pn2.dependencies.Add(unlitPass);

            m_ResourceList.Add(colorRT);
            m_ResourceList.Add(depthRT);
            m_ResourceList.Add(dirShadowMapRT);
            m_ResourceList.Add(otherShadowMapRT);
            m_ResourceList.Add(tn1.Resource);
            m_ResourceList.Add(targetRT);
            m_ResourceList.Add(cullingResultData);
            
            m_NodeList.Add(setupLightPass);
            m_NodeList.Add(setupGlobalPass);
            m_NodeList.Add(unlitPass);
            m_NodeList.Add(pn2);
            m_NodeList.Add(tn1);
            m_NodeList.Add(dirLightPass);
            m_NodeList.Add(otherLightPass);

            var p1 = (unlitPass.Parameters["depthAttachment"] as TextureParameterData);
            p1.UseDefault = true;
            p1.SetDefaultResource(depthRT);
            var p2 = (unlitPass.Parameters["colorAttachment"] as TextureParameterData);
            p2.UseDefault = true;
            p2.SetDefaultResource(colorRT);
            var p3 = unlitPass.Parameters["rendererList"];
            p3.UseDefault = false;

            var p4 = (pn2.Parameters["colorAttachment"] as TextureParameterData);
            p4.UseDefault = true;
            p4.SetDefaultResource(colorRT);
            var p5 = (pn2.Parameters["targetAttachment"] as TextureParameterData);
            p5.UseDefault = true;
            p5.SetDefaultResource(targetRT);

            var p6 = (dirLightPass.Parameters["shadowMap"] as TextureParameterData);
            p6.UseDefault = true;
            p6.SetDefaultResource(dirShadowMapRT);
            
            var p7 = (otherLightPass.Parameters["shadowMap"] as TextureParameterData);
            p7.UseDefault = true;
            p7.SetDefaultResource(otherShadowMapRT);
            
            var cullingResultParameterData = setupLightPass.Parameters["cullingResults"] as CullingResultParameterData;
            cullingResultParameterData.SetDefaultResource(cullingResultData);
            
            PortData.Connect(p3.Port, tn1.AttachTo);
        }

        #endregion

    }

}
