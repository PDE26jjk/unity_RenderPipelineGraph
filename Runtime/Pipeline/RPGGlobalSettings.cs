using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph {
    [DisplayInfo(name = "RPG Global Settings Asset", order = CoreUtils.Sections.section4 + 2)]
    [SupportedOnRenderPipeline(typeof(RPGRenderPipelineAsset))]
    [DisplayName("RPG")]
    public class RPGGlobalSettings : RenderPipelineGlobalSettings<RPGGlobalSettings, RPGRenderPipeline> {
        [SerializeField] RenderPipelineGraphicsSettingsContainer m_Settings = new();
        protected override List<IRenderPipelineGraphicsSettings> settingsList => m_Settings.settingsList;
#if UNITY_EDITOR
        static string defaultPath => $"Assets/{nameof(RPGGlobalSettings)}.asset";
        // If not writing this and creating a Settings file, many SRP features cannot be used, such as Blitter.
        public static RPGGlobalSettings Ensure(bool canCreateNewAsset = true) {

            RPGGlobalSettings currentInstance = GraphicsSettings.GetSettingsForRenderPipeline<RPGRenderPipeline>() as RPGGlobalSettings;

            if (RenderPipelineGlobalSettingsUtils.TryEnsure<RPGGlobalSettings, RPGRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset)) {
                if (currentInstance != null) {
                    // EditorUtility.SetDirty(currentInstance);
                    AssetDatabase.SaveAssetIfDirty(currentInstance);
                }

                return currentInstance;
            }

            return null;
        }
#endif
    }
}
