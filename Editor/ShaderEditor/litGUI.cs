using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
public class litGUI : ShaderGUI {
    MaterialEditor editor;
    Object[] materials;
    MaterialProperty[] properties;
    void SetProperty(string name, float value) {
        FindProperty(name, properties).floatValue = value;
    }
    void SetKeyword(string keyword, bool enabled) {
        if (enabled) {
            foreach (Material m in materials) {
                m.EnableKeyword(keyword);
            }
        }
        else {
            foreach (Material m in materials) {
                m.DisableKeyword(keyword);
            }
        }
    }
    void SetProperty(string name, string keyword, bool value) {
        SetProperty(name, value ? 1f : 0f);
        SetKeyword(keyword, value);
    }
    RenderQueue RenderQueue {
        set {
            foreach (Material m in materials) {
                m.renderQueue = (int)value;
            }
        }
    }
    bool PresetButton(string name) {
        if (GUILayout.Button(name)) {
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }
    public override void OnGUI(
        MaterialEditor materialEditor, MaterialProperty[] properties
    ) {

        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;
        EditorGUI.BeginChangeCheck();
        base.OnGUI(materialEditor, properties);
        if (EditorGUI.EndChangeCheck()) {
            var BRDF = FindProperty("_BRDF", properties, false);
            if (BRDF != null) {
                switch ((int)BRDF.floatValue) {
                    case 0:
                        SetKeyword("_BRDF_Unity", true);
                        SetKeyword("_BRDF_catlikeCoding", false);
                        break;
                    case 1:
                        SetKeyword("_BRDF_Unity", false);
                        SetKeyword("_BRDF_catlikeCoding", true);
                        break;
                }
            }
        }
        BakedEmission();
    }

    void BakedEmission() {
        EditorGUI.BeginChangeCheck();
        editor.LightmapEmissionProperty();
        if (EditorGUI.EndChangeCheck()) {
            foreach (Material m in editor.targets) {
                m.globalIlluminationFlags &=
                    ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }
    }
}
