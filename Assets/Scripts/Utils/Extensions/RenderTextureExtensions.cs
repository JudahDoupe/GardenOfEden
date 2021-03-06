﻿using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public static class RenderTextureExtensions
{
    public static Dictionary<RenderTexture, Texture2D[]> RTCache = new Dictionary<RenderTexture, Texture2D[]>();
    public static Dictionary<RenderTexture, AsyncGPUReadbackRequest> RTRequest = new Dictionary<RenderTexture, AsyncGPUReadbackRequest>();

    public static Texture2D[] CachedTextures(this RenderTexture rt)
    {
        if (!RTCache.TryGetValue(rt, out var tex))
        {
            rt.UpdateTextureCache();
            RTRequest[rt].WaitForCompletion();
            tex = RTCache[rt];
        }
        return tex;
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
                    {
                        RTCache[rt] = new[] {
                            new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false),
                            new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false),
                            new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false),
                            new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false),
                            new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false),
                            new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false)
                        };
                    }

                    for (var i = 0; i < 6; i++)
                    {
                        if(RTCache[rt][i] != null)
                        {
                            RTCache[rt][i].SetPixelData(request.GetData<Color>(i), 0);
                            RTCache[rt][i].Apply();
                        }
                    }
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

        return tex;
    }
    public static RenderTexture Initialize(this RenderTexture tex, float r = 0, float g = 0, float b = 0, float a = 0)
    {
        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/Initialize");
        var kernelId = cs.FindKernel("Initialize");
        cs.SetTexture(kernelId, "Map", tex);
        cs.SetFloats("Values", r, g, b, a);
        cs.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);

        return tex;
    }
    public static RenderTexture InitializeRandom(this RenderTexture tex, float seed, float min = -500, float max = 500)
    {
        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/Initialize");
        var kernelId = cs.FindKernel("InitializeRandom");
        cs.SetTexture(kernelId, "Map", tex);
        cs.SetFloat("Seed", seed);
        cs.SetFloat("Min", min);
        cs.SetFloat("Max", max);
        cs.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);

        return tex;
    }
    public static RenderTexture InitializeUvw(this RenderTexture tex)
    {
        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/Initialize");
        var kernelId = cs.FindKernel("InitializeUvw");
        cs.SetTexture(kernelId, "Map", tex);
        cs.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);

        return tex;
    }
    public static RenderTexture InitializeXyw(this RenderTexture tex)
    {
        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/Initialize");
        var kernelId = cs.FindKernel("InitializeXyw");
        cs.SetTexture(kernelId, "Map", tex);
        cs.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);

        return tex;
    }

    public static bool IsTextureBeingUpdated(this RenderTexture rt)
    {
        return RTRequest.TryGetValue(rt, out var req) && !req.done;
    }

    public static Color Sample(this RenderTexture rt, Coordinate coord)
    {
        var uvw = coord.uvw;
        var uv = uvw.xy - (0.5f / 512.0f);
        int w = (int)math.round(uvw.z);
        var texArray = rt.CachedTextures();
        var color = texArray[w].GetPixelBilinear(uv.x, uv.y, 0);
        return color;
    }
}