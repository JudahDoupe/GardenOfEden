using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class EnvironmentMap
{
    public EnvironmentMapType Type { get; }
    public EnvironmentMapMetaData MetaData { get; }
    public RenderTexture RenderTexture { get; }
    public Texture2D[] CachedTextures { get; private set; }
    public string Name => MetaData.Name;
    public int Channels => MetaData.Channels;
    public int Layers { get => RenderTexture.depth; set => ResetTexture(value); }
    public bool IsCacheBeingUpdated => !_request.done;

    private AsyncGPUReadbackRequest _request;

    public EnvironmentMap(EnvironmentMapType type)
    {
        Type = type;
        MetaData = Type.MetaData();
        RenderTexture = new RenderTexture(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, 0, MetaData.RenderTextureFormat, 0);
        ResetTexture(MetaData.Layers);
        CachedTextures = new Texture2D[MetaData.Layers];
        for (var i = 0; i < MetaData.Layers; i++)
        {
            CachedTextures[i] = new Texture2D(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, MetaData.TextureFormat, false);
        }
    }

    public void RefreshCache()
    {
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
                }
            });
        }
    }
    public void SetTextures(Texture2D[] textures)
    {
        RenderTexture.SetTexture(textures);
        CachedTextures = textures;
    }
    public void SetTextures(Texture2DArray tex)
    {
        Graphics.Blit(tex, RenderTexture);
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
        RenderTexture.filterMode = FilterMode.Trilinear;
        RenderTexture.enableRandomWrite = true;
        RenderTexture.isPowerOfTwo = true;
        RenderTexture.Create();
    }
}