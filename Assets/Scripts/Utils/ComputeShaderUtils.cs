using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeShaderUtils : MonoBehaviour
{
    public static int TextureSize = 512;
    public static int WorldSizeInMeters = 400;

    public static void ResetTexture(RenderTexture tex)
    {
        tex.Release();
        tex.enableRandomWrite = true;
        tex.Create();
    }

    public static Vector2 LocationToUv(Vector3 location)
    {
        var relativePosition = location - new Vector3(0, 0, -100);
        var uvPos = relativePosition / (WorldSizeInMeters / 2.0f);
        var uv = new Vector2(uvPos.x, uvPos.z);
        return (uv + new Vector2(1, 1)) / 2;
    }

    public static Vector2 LocationToXy(Vector3 location)
    {
        var uv = LocationToUv(location);
        return new Vector2(Mathf.FloorToInt(uv.x * 512), Mathf.FloorToInt(uv.y * 512));
    }


    public static Dictionary<RenderTexture, Texture2D> RTCache = new Dictionary<RenderTexture, Texture2D>();
    public static Dictionary<RenderTexture, AsyncGPUReadbackRequest> RTRequest = new Dictionary<RenderTexture, AsyncGPUReadbackRequest>();
    public static Texture2D GetCachedTexture(RenderTexture rt)
    {
        Texture2D tex;
        if (!RTCache.TryGetValue(rt, out tex))
        {
            tex = rt.ToTexture2D();
            RTCache.Add(rt, tex);
        }
        return tex;
    }
    public static void UpdateTexture(RenderTexture rt)
    {
        if (!RTRequest.TryGetValue(rt, out var req) || req.done)
        {
            RTRequest[rt] = AsyncGPUReadback.Request(rt, 0, request =>
            {
                if (request.hasError)
                {
                    Debug.Log("GPU readback error detected.");
                }
                else if (request.done)
                {
                    var buffer = request.GetData<Color>();

                    if (!RTCache.TryGetValue(rt, out Texture2D tex))
                        tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBAFloat, false);

                    tex.SetPixels(0,0, TextureSize, TextureSize, buffer.ToArray());
                    tex.Apply();
                    RTCache[rt] = tex;
                }
            });
        }

    }
}

public static class RenderTextureExtentions
{
    public static Texture2D ToTexture2D(this RenderTexture rt, TextureFormat format = TextureFormat.RGBAFloat)
    {
        RenderTexture currentRt = RenderTexture.active;
        Texture2D rtnTex = new Texture2D(ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize, format, false);
        RenderTexture.active = rt;

        rtnTex.ReadPixels(new Rect(0, 0, ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize), 0, 0);
        rtnTex.Apply();

        RenderTexture.active = currentRt;

        return rtnTex;
    }

    public static void LoadFromFile(this RenderTexture rt, string path, TextureFormat format = TextureFormat.RGBAFloat)
    {
        var tex = new Texture2D(ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize, format, false);
        tex.LoadRawTextureData(File.ReadAllBytes(path));
        tex.Apply();
        Graphics.Blit(tex, rt);
    }

    public static void SaveToFile(this RenderTexture rt, string path, TextureFormat format = TextureFormat.RGBAFloat)
    {
        var tex = rt.ToTexture2D(format);
        var bytes = tex.GetRawTextureData();
        File.WriteAllBytes(path, bytes);
    }
}