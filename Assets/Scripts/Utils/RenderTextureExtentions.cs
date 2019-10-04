using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderTextureExtentions
{
    public static Texture2D ToTexture2D(this RenderTexture rt)
    {
        RenderTexture currentRt = RenderTexture.active;
        Texture2D rtnTex = new Texture2D(ComputeShaderService.TextureSize, ComputeShaderService.TextureSize, TextureFormat.RGBA32, false);
        RenderTexture.active = rt;

        rtnTex.ReadPixels(new Rect(0, 0, ComputeShaderService.TextureSize, ComputeShaderService.TextureSize), 0, 0);
        rtnTex.Apply();

        RenderTexture.active = currentRt;

        return rtnTex;
    }
}
