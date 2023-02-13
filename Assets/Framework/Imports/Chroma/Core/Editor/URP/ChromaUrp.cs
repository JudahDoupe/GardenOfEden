#if CHROMA_URP
using UnityEditor.ShaderGraph;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using ShaderID = Unity.Rendering.Universal.ShaderUtils.ShaderID;
#endif

#if UNITY_2022_1_OR_NEWER
    using BaseGuiType = UnityEditor.ShaderGUI;
#else
    using BaseGuiType = UnityEditor.BaseShaderGUI;
#endif

namespace UnityEditor {
// ReSharper disable once UnusedType.Global
public class ChromaUrp : BaseShaderGUI {
    private BaseGuiType _shaderGUI;

#if UNITY_2021_2_OR_NEWER
    private ShaderID _shaderID = ShaderID.Unknown;
#endif

#if UNITY_2021_2_OR_NEWER
    public override void ValidateMaterial(Material material) {
        _shaderGUI = CreateInstance(material.shader);
        _shaderGUI.ValidateMaterial(material);
    }
#else
    public override void MaterialChanged(Material material) {
        _shaderGUI ??= CreateInstance(material.shader);
        _shaderGUI.MaterialChanged(material);
    }
#endif

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
        var material = materialEditorIn.target as Material;
        if (material == null) return;
        _shaderGUI = CreateInstance(material.shader);
        _shaderGUI.OnGUI(materialEditorIn, properties);
    }

    private BaseGuiType CreateInstance(Shader shader) {
#if UNITY_2021_2_OR_NEWER
        var shaderID = Unity.Rendering.Universal.ShaderUtils.GetShaderID(shader);
        if (_shaderGUI != null && _shaderID == shaderID) return _shaderGUI;
        _shaderID = shaderID;

        if (shader.IsShaderGraphAsset()) {
            switch (shaderID) {
                case ShaderID.SG_Lit:
                default:
                    return new ChromaShaderGraphLitGui();
                case ShaderID.SG_Unlit:
                    return new ChromaShaderGraphUnlitGui();
#if UNITY_2022_1_OR_NEWER
                case ShaderID.SG_Decal:
                    return new ChromaShaderGraphDecalGui();
#endif
            }
        }
#endif

        return new ChromaCodeGuiUrp();
    }
}
}
#endif