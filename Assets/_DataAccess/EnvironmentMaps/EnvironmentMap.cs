using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class EnvironmentMap
{
    public EnvironmentMapType Type { get; }
    public EnvironmentMapMetaData MetaData { get; }
    public RenderTexture RenderTexture { get; }
    public Texture2D[] CachedTextures { get; private set; }
    public string Name => MetaData.Name;
    public int Channels => MetaData.Channels;
    public int Layers { get => RenderTexture.volumeDepth; set => ResetTexture(value); }
    public bool IsCacheBeingUpdated => !_request.done;

    private AsyncGPUReadbackRequest _request;
    private List<Action> _cacheCallbacks = new List<Action>();

    public EnvironmentMap(EnvironmentMapType type)
    {
        Type = type;
        MetaData = Type.MetaData();
        RenderTexture = new RenderTexture(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, 0, MetaData.RenderTextureFormat, 0);
        ResetTexture(MetaData.Layers);
    }

    public void RefreshCache(Action callback = null)
    {
        if (callback != null && !_cacheCallbacks.Contains(callback))
        {
            _cacheCallbacks.Add(callback);
        }

        if (!IsCacheBeingUpdated && RenderTexture.IsCreated())
        {
            _request = AsyncGPUReadback.Request(RenderTexture, 0, request =>
            {
                if (request.hasError || RenderTexture == null || !RenderTexture.IsCreated())
                {
                    Debug.Log("GPU readback error detected.");
                }
                else if (request.done)
                {
                    for (var i = 0; i < Layers; i++)
                    {
                        switch (MetaData.RenderTextureFormat)
                        {
                            case RenderTextureFormat.RFloat:
                                CachedTextures[i].SetPixelData(request.GetData<float>(i), 0);
                                break;
                            case RenderTextureFormat.RGFloat:
                                CachedTextures[i].SetPixelData(request.GetData<float2>(i), 0);
                                break;
                            default:
                                CachedTextures[i].SetPixelData(request.GetData<float4>(i), 0);
                                break;

                        }
                        CachedTextures[i].Apply();
                    }

                    foreach (var action in _cacheCallbacks)
                    {
                        action.Invoke();
                    }
                    _cacheCallbacks.Clear();
                }
            });
        }
    }
    public void SetTextures(Texture2D[] textures)
    {
        CachedTextures = textures;
        Layers = textures.Length;

        var texture = new Texture2DArray(textures[0].width, textures[0].height, Layers, MetaData.GraphicsFormat, TextureCreationFlags.None);
        for (var i = 0; i < Layers; i++)
        {
            texture.SetPixels(textures[i].GetPixels(0), i, 0);
        }
        texture.Apply();

        Graphics.Blit(texture, RenderTexture);
    }
    public void SetTextures(Texture2DArray tex)
    {
        Layers = tex.depth;
        Graphics.Blit(tex, RenderTexture);
        RefreshCache();
    }

    public Color Sample(Coordinate coord) => Sample(coord.TextureUvw);
    public Color Sample(float3 uvw) => CachedTextures[(int)math.round(uvw.z)].GetPixelBilinear(uvw.x, uvw.y, 0);

    public Color SamplePoint(Coordinate coord) => SamplePoint(coord.TextureXyw);
    public Color SamplePoint(int3 xyw) => CachedTextures[xyw.z].GetPixel(xyw.x, xyw.y);

    public void ResetTexture() => ResetTexture(Layers);
    private void ResetTexture(int layers)
    {
        RenderTexture.Release();
        RenderTexture.dimension = TextureDimension.Tex2DArray;
        RenderTexture.volumeDepth = layers;
        RenderTexture.wrapMode = TextureWrapMode.Clamp;
        RenderTexture.filterMode = FilterMode.Bilinear;
        RenderTexture.enableRandomWrite = true;
        RenderTexture.isPowerOfTwo = true;
        RenderTexture.Create();

        CachedTextures = new Texture2D[layers];
        for (var i = 0; i < layers; i++)
        {
            CachedTextures[i] = new Texture2D(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, MetaData.TextureFormat, false);
        }
    }
}