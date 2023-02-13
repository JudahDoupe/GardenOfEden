#if CHROMA_HDRP
using UnityEditor.Rendering;
using UnityEditor.Rendering.HighDefinition;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace UnityEditor {
class ChromaUiBlock : MaterialUIBlock {
    private readonly ChromaDrawers _drawers = new();
    private readonly ExpandableBit _foldoutBit;

    public ChromaUiBlock(ExpandableBit expandableBit) {
        _foldoutBit = expandableBit;
    }

    public override void LoadMaterialProperties() { }

    public override void OnGUI() {
        using (var mainFoldout = new MaterialHeaderScope("Exposed Properties", (uint)_foldoutBit, materialEditor)) {
            if (mainFoldout.expanded) {
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
                    ChromaShaderGraphPropertyDrawers.DrawShaderGraphGUI(_drawers, materialEditor, properties,
                                                                        metadata.categoryDatas);
                } else {
                    ChromaPropertyDrawer.DrawProperties(properties, materialEditor, _drawers);
                }
            }
        }
    }
}
}
#endif