#if CHROMA_URP

using UnityEditor.ShaderGraph;
using UnityEngine;

namespace UnityEditor {
internal class ChromaShaderGraphUnlitGui : ShaderGraphUnlitGUI {
    #region Shared Shader Graph GUI code

    private readonly ChromaDrawers _drawers = new();
    private MaterialProperty[] _properties;

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
        _properties = properties;
        materialEditor = materialEditorIn;
        base.OnGUI(materialEditorIn, properties);
    }

    public override void DrawSurfaceInputs(Material material) {
        Material m = materialEditor.target as Material;
        Shader s = m.shader;
        string path = AssetDatabase.GetAssetPath(s);
        ShaderGraphMetadata metadata = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path)) {
            if (obj is ShaderGraphMetadata meta) {
                metadata = meta;
                break;
            }
        }

        if (metadata != null) {
            ChromaShaderGraphPropertyDrawers.DrawShaderGraphGUI(_drawers, materialEditor, _properties,
                                                                metadata.categoryDatas);
        } else {
            ChromaPropertyDrawer.DrawProperties(_properties, materialEditor, _drawers);
        }
    }

    #endregion
}
}

#endif