using Chroma;
using UnityEngine;

namespace UnityEditor {
// ReSharper disable once UnusedType.Global
public class Chroma : ShaderGUI {
    private ShaderGUI _shaderGUI;

#if UNITY_2021_2_OR_NEWER
    public override void ValidateMaterial(Material material) {
        _shaderGUI ??= CreateInstance(material);
        _shaderGUI.ValidateMaterial(material);
    }
#endif

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
        var material = materialEditorIn.target as Material;
        if (material == null) return;
        _shaderGUI ??= CreateInstance(material);
        _shaderGUI.OnGUI(materialEditorIn, properties);
    }

    private static ShaderGUI CreateInstance(Material material) {
        if (TagUtils.IsUrp(material)) {
#if CHROMA_URP
            return new ChromaUrp();
#else
            Debug.LogError("<color=yellow><b>Chroma</b></color>: URP is not installed. Falling back to vanilla Chroma UI.");
#endif
        }
        
        if (TagUtils.IsHdrp(material)) {
#if CHROMA_HDRP
            return new ChromaHdrp();
#else
            Debug.LogError("<color=yellow><b>Chroma</b></color>: HDRP is not installed. Falling back to vanilla Chroma UI");
#endif
        }

        return new ChromaCodeGui();
    }
}
}