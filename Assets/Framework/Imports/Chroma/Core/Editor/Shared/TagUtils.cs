using System;
using UnityEngine;

namespace Chroma {
public static class TagUtils {
    public static bool IsShaderGraph(Material material) {
        return string.Equals(material.GetTag("ShaderGraphShader", false, "None"), "true",
                             StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsUrp(Material material) {
        return string.Equals(material.GetTag("RenderPipeline", false, "None"), "UniversalPipeline");
    }
    
    public static bool IsHdrp(Material material) {
        return string.Equals(material.GetTag("RenderPipeline", false, "None"), "HDRenderPipeline");
    }
}
}