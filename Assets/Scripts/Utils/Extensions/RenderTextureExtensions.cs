using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public static class RenderTextureExtensions
{
    public static Dictionary<RenderTexture, Texture2D> RTCache = new Dictionary<RenderTexture, Texture2D>();
    public static Dictionary<RenderTexture, AsyncGPUReadbackRequest> RTRequest = new Dictionary<RenderTexture, AsyncGPUReadbackRequest>();

    public static Texture2D CachedTexture(this RenderTexture rt)
    {
        Texture2D tex;
        if (!RTCache.TryGetValue(rt, out tex))
        {
            tex = rt.ToTexture2D();
            RTCache.Add(rt, tex);
        }
        return tex;
    }
    public static Texture2D ToTexture2D(this RenderTexture rt, TextureFormat format = TextureFormat.RGBAFloat)
    {
        RenderTexture currentRt = RenderTexture.active;
        Texture2D rtnTex = new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, format, false);
        RenderTexture.active = rt;

        rtnTex.ReadPixels(new Rect(0, 0, EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize), 0, 0);
        rtnTex.Apply();

        RenderTexture.active = currentRt;

        return rtnTex;
    }
    public static void UpdateTextureCache(this RenderTexture rt)
    {
        if (!rt.IsTextureBeingUpdated())
        {
            RTRequest[rt] = AsyncGPUReadback.Request(rt, 0, request =>
            {
                if (request.hasError)
                {
                    Debug.Log("GPU readback error detected.");
                }
                else if (request.done)
                {
                    if (!RTCache.ContainsKey(rt))
                        RTCache[rt] = new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false);

                    RTCache[rt].SetPixelData(request.GetData<Color>(), 0);
                    RTCache[rt].Apply();
                }
            });
        }

    }
    public static RenderTexture ResetTexture(this RenderTexture tex)
    {
        tex.Release(); 
        tex.dimension = TextureDimension.Tex2DArray;
        tex.volumeDepth = 6;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Trilinear;
        tex.enableRandomWrite = true;
        tex.isPowerOfTwo = true;
        tex.Create();

        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/Clear");
        var kernelId = cs.FindKernel("Clear");
        cs.SetTexture(kernelId, "Map", tex);
        cs.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);

        return tex;
    }
    public static bool IsTextureBeingUpdated(this RenderTexture rt)
    {
        return RTRequest.TryGetValue(rt, out var req) && !req.done;
    }


    public static void LoadFromFile(this RenderTexture rt, string path, TextureFormat format = TextureFormat.RGBAFloat)
    {
        Texture2D tex;  
        tex = new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, format, false);
        tex.LoadRawTextureData(File.ReadAllBytes(Application.persistentDataPath + '/' + path));
        tex.Apply();
        Graphics.Blit(tex, rt);
    }
    public static void SaveToFile(this RenderTexture rt, string path, TextureFormat format = TextureFormat.RGBAFloat)
    {
        var tex = rt.ToTexture2D(format);
        var bytes = tex.GetRawTextureData();
        File.WriteAllBytes(Application.persistentDataPath + path, bytes);
    }


}