using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class EnvironmentMap
{
    private readonly List<Action> _cacheCallbacks = new();

    private AsyncGPUReadbackRequest _request;

    public EnvironmentMap(string planetName, string mapName, int layers = 6, int channels = 1)
    {
        PlanetName = planetName;
        Name = mapName;
        Channels = channels;
        RenderTexture = new RenderTexture(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, 0, RenderTextureFormat, 0);
        ResetTexture(layers);
    }

    public EnvironmentMap(EnvironmentMapDbData dbData)
    {
        PlanetName = dbData.PlanetName;
        Name = dbData.MapName;
        Channels = dbData.Channels;
        RenderTexture = new RenderTexture(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, 0, RenderTextureFormat, 0);
        ResetTexture(dbData.Layers);
    }

    public RenderTexture RenderTexture { get; }
    public Texture2D[] CachedTextures { get; private set; }
    public string PlanetName { get; }
    public string Name { get; }
    public int Channels { get; }

    public int Layers
    {
        get => RenderTexture.volumeDepth;
        set => SetLayers(value);
    }

    public RenderTextureFormat RenderTextureFormat
        => Channels switch
        {
            1 => RenderTextureFormat.RFloat,
            2 => RenderTextureFormat.RGFloat,
            _ => RenderTextureFormat.ARGBFloat
        };

    public TextureFormat TextureFormat
        => Channels switch
        {
            1 => TextureFormat.RFloat,
            2 => TextureFormat.RGFloat,
            _ => TextureFormat.RGBAFloat
        };

    public GraphicsFormat GraphicsFormat
        => Channels switch
        {
            1 => GraphicsFormat.R32_SFloat,
            2 => GraphicsFormat.R32G32_SFloat,
            _ => GraphicsFormat.R32G32B32A32_SFloat
        };

    public bool IsCacheBeingUpdated => !_request.done;

    public EnvironmentMapDbData ToDbData()
        => new()
        {
            PlanetName = PlanetName,
            MapName = Name,
            Channels = Channels,
            Layers = Layers
        };

    public Task<Texture2D[]> RefreshCacheAsync()
    {
        var refreshTaskSource = new TaskCompletionSource<Texture2D[]>();
        RefreshCache(() => { refreshTaskSource.TrySetResult(CachedTextures); });
        return refreshTaskSource.Task;
    }

    public void RefreshCache(Action callback = null)
    {
        if (callback != null && !_cacheCallbacks.Contains(callback)) _cacheCallbacks.Add(callback);

        if (!IsCacheBeingUpdated && RenderTexture.IsCreated())
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
                        switch (RenderTextureFormat)
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

                    foreach (var action in _cacheCallbacks) action.Invoke();
                    _cacheCallbacks.Clear();
                }
            });
    }

    public void SetTextures(Texture2D[] textures)
    {
        CachedTextures = textures;

        var texture = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, GraphicsFormat, TextureCreationFlags.None);
        for (var i = 0; i < textures.Length; i++) texture.SetPixels(textures[i].GetPixels(0), i, 0);
        texture.Apply();
        SetTextures(texture);
    }

    public void SetTextures(Texture2DArray tex)
    {
        Layers = tex.depth;
        Graphics.Blit(tex, RenderTexture);
        RefreshCache();
    }

    public void SetTextures(EnvironmentMap map)
    {
        Layers = map.Layers;
        Graphics.Blit(map.RenderTexture, RenderTexture);
        RefreshCache();
    }

    public Color Sample(Coordinate coord) => Sample(coord.TextureUvw);
    public Color Sample(float3 uvw) => CachedTextures[(int)math.round(uvw.z)].GetPixelBilinear(uvw.x, uvw.y, 0);

    public Color SamplePoint(Coordinate coord) => SamplePoint(coord.TextureXyw);
    public Color SamplePoint(int3 xyw) => CachedTextures[xyw.z].GetPixel(xyw.x, xyw.y);

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
        for (var i = 0; i < layers; i++) CachedTextures[i] = new Texture2D(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, TextureFormat, false);
    }

    private void SetLayers(int layers)
    {
        if (Layers == layers)
            return;

        RenderTexture.Release();
        RenderTexture.dimension = TextureDimension.Tex2DArray;
        RenderTexture.volumeDepth = layers;
        RenderTexture.wrapMode = TextureWrapMode.Clamp;
        RenderTexture.filterMode = FilterMode.Bilinear;
        RenderTexture.enableRandomWrite = true;
        RenderTexture.isPowerOfTwo = true;
        RenderTexture.Create();

        var cacheQueue = new Queue<Texture2D>(CachedTextures);
        CachedTextures = new Texture2D[layers];
        for (var i = 0; i < layers; i++)
            CachedTextures[i] = cacheQueue.TryDequeue(out var tex)
                ? tex
                : new Texture2D(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, TextureFormat, false);
    }
}

[Serializable]
public class EnvironmentMapDbData
{
    public string PlanetName;
    public string MapName;
    public int Channels = 1;
    public int Layers = 6;
}