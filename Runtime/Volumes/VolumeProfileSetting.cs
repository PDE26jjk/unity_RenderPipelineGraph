using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PDE {
    public class VolumeProfileSetting {
        internal static VolumeProfile GetOrCreateDefaultVolumeProfile() {
            if (GraphicsSettings.defaultRenderPipeline is RPGRenderPipelineAsset renderPipelineAsset) {
                ref VolumeProfile defaultVolumeProfile = ref renderPipelineAsset.defaultVolumeProfile;
#if UNITY_EDITOR
                if (defaultVolumeProfile is null) {
                    const string k_DefaultVolumeProfileName = "DefaultVolumeProfile";
                    const string k_DefaultVolumeProfilePath = "Assets/" + k_DefaultVolumeProfileName + ".asset";
                    defaultVolumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(k_DefaultVolumeProfilePath);
                    if (defaultVolumeProfile == null || defaultVolumeProfile.Equals(null)) {

                        VolumeProfile assetCreated = ScriptableObject.CreateInstance<VolumeProfile>();
                        Debug.Assert(assetCreated);

                        assetCreated.name = k_DefaultVolumeProfileName;
                        AssetDatabase.CreateAsset(assetCreated, k_DefaultVolumeProfilePath);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        defaultVolumeProfile = assetCreated;
                    }
                }
#endif
                return defaultVolumeProfile;
            }
            return null;
        }
    }
}
