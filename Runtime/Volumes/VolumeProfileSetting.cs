using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PDE {
    public class VolumeProfileSetting {
        static VolumeProfile defaultProfile;
        internal static VolumeProfile GetOrCreateDefaultVolumeProfile() {

            const string k_DefaultVolumeProfileName = "DefaultVolumeProfile";
            const string k_DefaultVolumeProfilePath = "Assets/" + k_DefaultVolumeProfileName + ".asset";
            defaultProfile ??=  AssetDatabase.LoadAssetAtPath<VolumeProfile>(k_DefaultVolumeProfilePath);
#if UNITY_EDITOR
            if (defaultProfile == null || defaultProfile.Equals(null)) {

                VolumeProfile assetCreated = ScriptableObject.CreateInstance<VolumeProfile>();
                Debug.Assert(assetCreated);

                assetCreated.name = k_DefaultVolumeProfileName;
                AssetDatabase.CreateAsset(assetCreated, k_DefaultVolumeProfilePath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                defaultProfile = assetCreated;

            }
#endif
            return defaultProfile;
        }
        
    }
}
