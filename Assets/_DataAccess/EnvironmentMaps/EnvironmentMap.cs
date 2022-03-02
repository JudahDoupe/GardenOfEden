using UnityEngine;

public class EnvironmentMap
{
    public EnvironmentMapType Type { get; }
    public EnvironmentMapMetaData MetaData { get; }
    public RenderTexture RenderTexture { get; }
    public Texture2D[] CachedTextures => RenderTexture.CachedTextures();
    public string Name => MetaData.Name;
    public int Channels => MetaData.Channels;
    public int Layers => MetaData.Layers;

    public EnvironmentMap(EnvironmentMapType type)
    {
        Type = type;
        MetaData = Type.MetaData();
        RenderTexture = new RenderTexture(512, 512, 0, MetaData.RenderTextureFormat, 0).ResetTexture(Layers);
    }

    public void RefreshCache()
    {
        RenderTexture.UpdateTextureCache();
    }

    public void SetTextures(Texture2D[] textures)
    {
        RenderTexture.SetTexture(textures);
    }
}