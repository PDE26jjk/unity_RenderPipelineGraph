using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RenderPipelineGraph;
using RenderPipelineGraph.Attribute;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using Object = System.Object;

public static class RenderGraphUtils {
    static readonly Dictionary<Type, MethodInfo> AddRasterRenderPasses = new();
    static MethodInfo _addComputeRenderPass;
    static MethodInfo rgAddRasterRenderPassMethodInfo;
    static readonly Dictionary<Type, Type> passDataTypes = new();

    static Dictionary<string, ShaderTagId> m_ShaderTagIdsMap = new();
    public static ShaderTagId GetShaderTagId(string shaderTagIdStr) {
        if (!m_ShaderTagIdsMap.TryGetValue(shaderTagIdStr, out var shaderTagId)) {
            m_ShaderTagIdsMap[shaderTagIdStr] = shaderTagId = new ShaderTagId(shaderTagIdStr);
        }
        return shaderTagId;
    }

    static Dictionary<string, int> m_ShaderPropertyIdsMap = new();
    public static int GetShaderPropertyId(string shaderPropertyIdStr) {
        if (!m_ShaderPropertyIdsMap.TryGetValue(shaderPropertyIdStr, out var shaderPropertyId)) {
            m_ShaderPropertyIdsMap[shaderPropertyIdStr] = shaderPropertyId = Shader.PropertyToID(shaderPropertyIdStr);
        }
        return shaderPropertyId;
    }

    // get PassData Class from pass:
    public static Type GetPassDataType(RPGPass pass) {
        string passInputTypeName = "PassData";
        Type passType = pass.GetType();
        if (passDataTypes.TryGetValue(passType, out var value)) return value;
        Type passInputType = passType.GetNestedType(passInputTypeName, BindingFlags.Public | BindingFlags.NonPublic);
        passDataTypes[passType] = passInputType;
        return passInputType;
    }

    // get method from RenderGraph:
    // public IRasterRenderGraphBuilder AddRasterRenderPass<PassData>(string passName, out PassData passData, ProfilingSampler sampler
    // #if !CORE_PACKAGE_DOCTOOLS
    //         ,[CallerFilePath] string file = "",
    //         [CallerLineNumber] int line = 0) where PassData : class, new()
    // #endif
    // )
    public static MethodInfo GetAddRasterRenderPassMethodInfo(RenderGraph renderGraph, RPGPass pass) {

        Type passDataType = GetPassDataType(pass);

        if (AddRasterRenderPasses.TryGetValue(passDataType, out var value)) return value;
        string addRasterRenderPassName = nameof(renderGraph.AddRasterRenderPass);
        rgAddRasterRenderPassMethodInfo ??= renderGraph.GetType().GetMethods().First(info =>
            info.Name == addRasterRenderPassName && info.GetParameters().Length >= 3 && info.GetParameters()[2].ParameterType == typeof(ProfilingSampler));
        MethodInfo methodInfo = rgAddRasterRenderPassMethodInfo.MakeGenericMethod(passDataType);
        AddRasterRenderPasses[passDataType] = methodInfo;
        return methodInfo;
    }

    static MethodInfo GetDefaultProfilingSampler;
    public static Object[] MakeAddRenderPassParm(RenderGraph renderGraph, RPGPass pass) {
        GetDefaultProfilingSampler ??= typeof(RenderGraph).GetMethod(nameof(GetDefaultProfilingSampler), BindingFlags.NonPublic | BindingFlags.Instance);
        return new Object[] {
            pass.Name,
            null,
            GetDefaultProfilingSampler.Invoke(renderGraph, new object[] {
                pass.Name
            })
#if !CORE_PACKAGE_DOCTOOLS
            ,
            "RPG MakeAddRenderPassParm",
            0,
#endif
        };
    }

    static readonly Dictionary<Type, MethodInfo> renderFunWithPassDatas = new();
    static readonly Dictionary<Type, Delegate> RenderFuns = new();
    static readonly Dictionary<PassNodeType, MethodInfo> rgSetRenderFuncMethodInfos = new();

    // replace builder.SetRenderFunc
    public static void SetRenderFunc(IRasterRenderGraphBuilder builder, RPGPass pass) {
        Type passType = pass.GetType();
        Type passDataType = GetPassDataType(pass);
        Type rgContextType = pass.PassType switch {
            PassNodeType.Legacy => typeof(RenderGraphContext),
            PassNodeType.Unsafe => typeof(UnsafeGraphContext),
            PassNodeType.Raster => typeof(RasterGraphContext),
            PassNodeType.Compute => typeof(ComputeGraphContext),
            _ => throw new ArgumentOutOfRangeException()
        };

        // get builder.SetRenderFunc from RenderGraph Builder
        if (!renderFunWithPassDatas.TryGetValue(passType, out MethodInfo renderFunWithPassData)) {
            string setRenderFuncName = nameof(builder.SetRenderFunc);

            if (!rgSetRenderFuncMethodInfos.TryGetValue(pass.PassType, out var rgSetRenderFuncMethodInfo)) {

                rgSetRenderFuncMethodInfos[pass.PassType] = rgSetRenderFuncMethodInfo = builder.GetType().GetMethods().First(methodInfo => {
                    if (methodInfo.Name == setRenderFuncName) {
                        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                        if (parameterInfos[0].ParameterType.GetGenericArguments()[1] == rgContextType) {
                            return true;
                        }
                    }
                    return false;
                });
            }
            renderFunWithPassData = rgSetRenderFuncMethodInfo.MakeGenericMethod(passDataType);
            renderFunWithPassDatas[passType] = renderFunWithPassData;
        }

        // get method in pass:
        // public static Record(PassData data, ContextType renderGraphContext)
        if (!RenderFuns.TryGetValue(passType, out Delegate renderFun)) {
            var renderFunName = "Record";
            var renderFunType = passType.GetMethods().First(m => m.Name == renderFunName);
            Type[] typeArguments = renderFunType.GetParameters().Select(t => t.ParameterType).ToArray();
            Type baseRenderFuncWithPassDataType = typeof(BaseRenderFunc<,>).MakeGenericType(typeArguments);

            renderFun = Delegate.CreateDelegate(baseRenderFuncWithPassDataType, null, renderFunType);
            RenderFuns[passType] = renderFun;
        }

        // like builder.SetRenderFunc(renderFun)
        renderFunWithPassData.Invoke(builder, new object[] {
            renderFun
        });
    }
    public static void LoadPassData(PassNodeData passNodeData, object passData, IBaseRenderGraphBuilder builder, RenderGraph renderGraph, Camera camera) {
        RPGPass pass = passNodeData.Pass;
        Type passDataType = GetPassDataType(pass);

        foreach (var keyValuePair in passNodeData.Parameters) {
            string filedName = keyValuePair.Key;
            RPGParameterData parameterData = keyValuePair.Value;
            parameterData.passTypeFieldInfo ??= passDataType.GetField(filedName);
            // if (parameterData.passTypeFieldInfo == null) {
            //     Debug.LogError($"{passNodeData.exposedName}.{parameterData.Name} Loading Error.");
            // }
            parameterData.LoadDataField(passData,builder);
        }

        pass.Setup(passData, camera, renderGraph, builder);

    }
}
