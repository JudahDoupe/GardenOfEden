using LiteDB;
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

    public static Texture2D[] CachedTextures(this RenderTexture rt, bool updateCache = false)
    {
        if (updateCache || !RTCache.TryGetValue(rt, out var tex))
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
    public static void SetTexture(this RenderTexture rt, Texture2D[] textures)
    {
        RTCache[rt] = textures;

        var texture = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, rt.graphicsFormat, TextureCreationFlags.None);
        for (var i = 0; i < rt.volumeDepth; i++)
        {
            texture.SetPixels(textures[i].GetPixels(0), i, 0);
        }
        texture.Apply();
        
        rt.SetTexture(texture);
    }
    public static void SetTexture(this RenderTexture rt, Texture2DArray newTexture)
    {
        Graphics.Blit(newTexture, rt);
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

    public static bool IsTextureBeingUpdated(this RenderTexture rt)
    {
        return RTRequest.TryGetValue(rt, out var req) && !req.done;
    }

    public static Color Sample(this RenderTexture rt, float u, float v)
    {
        var tex = rt.CachedTexture();
        var color = tex.GetPixelBilinear(u, v, 0);
        return color;
    }
}