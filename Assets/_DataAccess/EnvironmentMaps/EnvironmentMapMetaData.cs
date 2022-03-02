using System;
using UnityEngine;

public class EnvironmentMapMetaData
{
    public string Name { get; }
    public int Channels { get; }
    public int Layers { get; }
    public RenderTextureFormat RenderTextureFormat => Channels switch
    {
        1 => RenderTextureFormat.RFloat,
        2 => RenderTextureFormat.RGFloat,
        _ => RenderTextureFormat.ARGBFloat
    };
    public TextureFormat TextureFormat => Channels switch
    {
        1 => TextureFormat.RFloat,
        2 => TextureFormat.RGFloat,
        _ => TextureFormat.RGBAFloat,
    };

    public EnvironmentMapMetaData(string name, int channels = 1, int layers = 6)
    {
        Name = name;
        Channels = channels;
        Layers = layers;
    }
}
public class EnvironmentMapMetaDataAttribute : Attribute
{
    public EnvironmentMapMetaData MetaData { get; }
    public string Name => MetaData.Name;
    public int Channels => MetaData.Channels;
    public int Layers => MetaData.Layers;
    public RenderTextureFormat RenderTextureFormat => MetaData.RenderTextureFormat;
    public TextureFormat TextureFormat => MetaData.TextureFormat;

    public EnvironmentMapMetaDataAttribute(string name, int channels = 1, int layers = 6)
    {
        MetaData = new EnvironmentMapMetaData(name, channels, layers);
    }
}