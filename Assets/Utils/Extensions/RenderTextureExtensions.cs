using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
    public static Texture2D CachedTexture(this RenderTexture rt)
    {
        return CachedTextures(rt)[0];
    }
    public static void UpdateTextureCache(this RenderTexture rt)
    {
        if (!rt.IsTextureBeingUpdated() && rt.IsCreated())
        {
            RTRequest[rt] = AsyncGPUReadback.Request(rt, 0, request =>
            {
                if (request.hasError || rt == null || !rt.IsCreated())
                {
                    Debug.Log("GPU readback error detected.");
                }
                else if (request.done)
                {
                    if (!RTCache.ContainsKey(rt))
                    {
                        var format = rt.format switch
                        {
                            RenderTextureFormat.RFloat => TextureFormat.RFloat,
                            RenderTextureFormat.RGFloat => TextureFormat.RGFloat,
                            _ => TextureFormat.RGBAFloat,
                        };

                        var list = new List<Texture2D>();
                        for (var i = 0; i< rt.volumeDepth; i++)
                        {
                            list.Add(new Texture2D(rt.width, rt.height, format, false));
                        }
                        RTCache[rt] = list.ToArray();
                    }

                    for (var i = 0; i < rt.volumeDepth; i++)
                    {
                        if(RTCache[rt][i] != null)
                        {
                            switch (rt.format)
                            {
                                case RenderTextureFormat.RFloat:
                                    RTCache[rt][i].SetPixelData(request.GetData<float>(i), 0);
                                    break;
                                case RenderTextureFormat.RGFloat:
                                    RTCache[rt][i].SetPixelData(request.GetData<float2>(i), 0);
                                    break;
                                default:
                                    RTCache[rt][i].SetPixelData(request.GetData<float4>(i), 0);
                                    break;

                            }
                            RTCache[rt][i].Apply();
                        }
                    }
                }
            });
        }

    }
    public static void ClearCache(this RenderTexture rt)
    {
        if (RTCache.ContainsKey(rt))
        {
            RTCache.Remove(rt);
        }
        if (RTRequest.ContainsKey(rt))
        {
            RTRequest.Remove(rt);
        }
    }

    public static RenderTexture ResetTexture(this RenderTexture tex, int layers)
    {
        tex.Release(); 
        tex.dimension = TextureDimension.Tex2DArray;
        tex.volumeDepth = layers;
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
        cs.Dispatch(kernelId, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);

        return tex;
    }
    public static RenderTexture Initialize(this RenderTexture tex, Color[][] colors)
    {
        var texture = new Texture2DArray(colors.Length, colors[0].Length, 6, tex.graphicsFormat, TextureCreationFlags.None);
        for (int w = 0; w < 6; w++)
        {
            texture.SetPixels(colors.SelectMany(x => x).ToArray(), w);
        }
        texture.Apply();
        Graphics.Blit(texture, tex);
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
        cs.Dispatch(kernelId, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);

        return tex;
    }
    public static RenderTexture InitializeUvw(this RenderTexture tex)
    {
        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/Initialize");
        var kernelId = cs.FindKernel("InitializeUvw");
        cs.SetTexture(kernelId, "Map", tex);
        cs.Dispatch(kernelId, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);

        return tex;
    }
    public static RenderTexture InitializeXyw(this RenderTexture tex)
    {
        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/Initialize");
        var kernelId = cs.FindKernel("InitializeXyw");
        cs.SetTexture(kernelId, "Map", tex);
        cs.Dispatch(kernelId, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);

        return tex;
    }

    public static bool IsTextureBeingUpdated(this RenderTexture rt)
    {
        return RTRequest.TryGetValue(rt, out var req) && !req.done;
    }

    public static Color Sample(this RenderTexture rt, Coordinate coord)
    {
        var uvw = coord.TextureUvw;
        var uv = uvw.xy - (0.5f / 512.0f);
        int w = (int)math.round(uvw.z);
        var texArray = rt.CachedTextures();
        var color = texArray[w].GetPixelBilinear(uv.x, uv.y, 0);
        return color;
    }
    public static Color SamplePoint(this RenderTexture rt, Coordinate coord)
    {
        var xyw = coord.TextureXyw;
        var texArray = rt.CachedTextures();
        var color = texArray[xyw.z].GetPixel(xyw.x, xyw.y);
        return color;
    }
    public static Color Sample(this RenderTexture rt, int3 xyw)
    {
        var texArray = rt.CachedTextures();
        var color = texArray[xyw.z].GetPixel(xyw.x, xyw.y);
        return color;
    }
    public static Color Sample(this RenderTexture rt, int x, int y)
    {
        var tex = rt.CachedTexture();
        var color = tex.GetPixel(x, y, 0);
        return color;
    }
    public static Color Sample(this RenderTexture rt, float u, float v)
    {
        var tex = rt.CachedTexture();
        var color = tex.GetPixelBilinear(u, v, 0);
        return color;
    }
}