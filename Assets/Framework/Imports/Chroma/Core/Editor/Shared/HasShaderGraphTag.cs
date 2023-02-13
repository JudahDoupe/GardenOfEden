using UnityEngine;

namespace Chroma {
// We need a check if the shader is SG or code without depending on SG assembly.
public static class HasShaderGraphTag {
    public static bool Check(Material material) {
        // From `GraphUtil.cs`.
        var shaderGraphTag = material.GetTag("ShaderGraphShader", false, null);
        return !string.IsNullOrEmpty(shaderGraphTag);
    }
}
}