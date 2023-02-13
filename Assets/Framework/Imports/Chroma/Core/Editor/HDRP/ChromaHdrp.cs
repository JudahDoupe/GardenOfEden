#if CHROMA_HDRP
using System.Reflection;
using Chroma;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEditor.Rendering.HighDefinition.ShaderGraph;
using UnityEngine;
using UnityEditor.ShaderGraph;

#if UNITY_2022_1_OR_NEWER
using ShaderID = UnityEngine.Rendering.HighDefinition.HDMaterial.ShaderID;
#else
using ShaderID = UnityEditor.Rendering.HighDefinition.HDShaderUtils.ShaderID;
#endif

namespace UnityEditor {
// ReSharper disable once UnusedType.Global
public class ChromaHdrp : HDShaderGUI {
    private HDShaderGUI _shaderGUI;
    private ShaderID _shaderID = ShaderID.Count_All;

#if UNITY_2021_2_OR_NEWER
    public override void ValidateMaterial(Material material) {
        CreateInstance(material);
        _shaderGUI.ValidateMaterial(material);
    }
#else
    protected override void SetupMaterialKeywordsAndPass(Material material) {
        CreateInstance(material);
        // Use reflection to call the base method SetupMaterialKeywordsAndPass.
        var method = typeof(HDShaderGUI).GetMethod("SetupMaterialKeywordsAndPass",
                                                   BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(_shaderGUI, new object[] { material });
    }
#endif

    protected override void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props) {
        var material = (Material)materialEditor.target;
        CreateInstance(material);
        _shaderGUI.OnGUI(materialEditor, props);
    }

    private void CreateInstance(Material material) {
        if (material.shader.TryGetMetadataOfType<HDMetadata>(out var metadata)) {
            if (_shaderGUI != null && _shaderID == metadata.shaderID) return;
            _shaderID = metadata.shaderID;

#if UNITY_2021_2_OR_NEWER
            if (material.shader.IsShaderGraphAsset()) {
#else
            if (TagUtils.IsShaderGraph(material)) {
#endif
                switch (metadata.shaderID) {
                    case ShaderID.SG_Lit:
                    case ShaderID.SG_Eye:
                    case ShaderID.SG_Hair:
                    case ShaderID.SG_Fabric:
                    default:
                        _shaderGUI = new ChromaHdrpLit();
                        break;
                    case ShaderID.SG_Unlit:
                    case ShaderID.Unlit:
                        _shaderGUI = new ChromaHdrpUnlit();
                        break;
                    case ShaderID.SG_Decal:
                    case ShaderID.Decal:
                        _shaderGUI = new ChromaHdrpDecal();
                        break;
                }
            }
        } else {
            // Code-based shaders.
            // TODO: Implement.
            _shaderGUI = new ChromaHdrpLit();
        }
    }
}
}

class ChromaHdrpLit : LightingShaderGraphGUI {
    public ChromaHdrpLit() {
        uiBlocks.RemoveAll(b => b is ShaderGraphUIBlock);
        uiBlocks.Insert(1, new ChromaUiBlock(MaterialUIBlock.ExpandableBit.User0));

        const MaterialUIBlock.ExpandableBit bit = MaterialUIBlock.ExpandableBit.Transparency;
        // From `LitShaderGraphGUI.cs`:
        uiBlocks.Insert(1, new TransparencyUIBlock(bit, TransparencyUIBlock.Features.Refraction));
    }
}

public class ChromaHdrpUnlit : UnlitShaderGraphGUI {
    public ChromaHdrpUnlit() {
        uiBlocks.RemoveAll(b => b is ShaderGraphUIBlock);
        uiBlocks.Insert(1, new ChromaUiBlock(MaterialUIBlock.ExpandableBit.User0));
    }
}

public class ChromaHdrpDecal : DecalShaderGraphGUI {
    public ChromaHdrpDecal() {
        uiBlocks.RemoveAll(b => b is ShaderGraphUIBlock);
        uiBlocks.Insert(1, new ChromaUiBlock(MaterialUIBlock.ExpandableBit.User0));
    }
}

#endif