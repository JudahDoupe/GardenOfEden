using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteDB;
using System.IO;
using System;

public enum EnvironmentMapType
{
    [EnvironmentMapMetaData(name: "WaterMap", channels: 4)]
    WaterMap,
    [EnvironmentMapMetaData(name: "WaterSourceMap", channels: 4)]
    WaterSourceMap,
    [EnvironmentMapMetaData(name: "LandHeightMap")]
    LandHeightMap,
    [EnvironmentMapMetaData(name: "PlateThicknessMaps")]
    PlateThicknessMaps,
    [EnvironmentMapMetaData(name: "TmpPlateThicknessMaps")]
    TmpPlateThicknessMaps,
    [EnvironmentMapMetaData(name: "ContinentalIdMap")]
    ContinentalIdMap,
    [EnvironmentMapMetaData(name: "TmpContinentalIdMap")]
    TmpContinentalIdMap,
}

public class EnvironmentMap
{
    public string Name { get; }
    public int Channels { get; }
    public int Layers { get; private set; }
    public EnvironmentMapType Type { get; }
    public EnvironmentMapMetaDataAttribute MetaData { get; }
    public RenderTexture RenderTexture { get; }
    public Texture2D[] CachedTextures => RenderTexture.CachedTextures();

    public EnvironmentMap(EnvironmentMapType type)
    {
        Type = type;
        MetaData = Type.MetaData();
        Name = MetaData.Name;
        Channels = MetaData.Channels;
        Layers = MetaData.Layers;
        RenderTexture = new RenderTexture(512, 512, 0, MetaData.RenderTextureFormat, 0).ResetTexture(Layers);
    }

    public void RefreshCache()
    {
        RenderTexture.UpdateTextureCache();
    }

    public void SetTextures(Texture2D[] textures)
    {
        Layers = textures.Count();
        RenderTexture.SetTexture(textures);
    }
}

public class EnvironmentMapDataStore : MonoBehaviour
{
    public static RenderTexture WaterSourceMap { get; set; }
    public static RenderTexture WaterMap { get; set; }
    public static RenderTexture LandHeightMap { get; set; }
    public static RenderTexture PlateThicknessMaps { get; set; }
    public static RenderTexture TmpPlateThicknessMaps { get; set; }
    public static RenderTexture ContinentalIdMap { get; set; }
    public static RenderTexture TmpContinentalIdMap { get; set; }

    public bool Reload = false;

    void Awake()
    {
        WaterMap = NewTexture(4, 6);
        WaterSourceMap = NewTexture(4, 6);
        LandHeightMap = NewTexture(1, 6);
        PlateThicknessMaps = NewTexture(1, 1);
        TmpPlateThicknessMaps = NewTexture(1, 1); 
        ContinentalIdMap = NewTexture(1, 6);
        TmpContinentalIdMap = NewTexture(1, 6);
        
        Load();
    }

    void Update()
    {
        if (Reload)
        {
            Load();
            Reload = false;
        }
    }

    public static void Load()
    {
        using var db = new LiteDatabase($@"{Application.persistentDataPath}\Environment.db");
        var fs = db.GetStorage<string>();
        var maps = new Dictionary<string, RenderTexture>()
        {
            { "WaterMap", WaterMap },
            { "WaterSourceMap", WaterSourceMap },
            { "LandHeightMap", LandHeightMap },
            { "PlateThicknessMaps", PlateThicknessMaps },
            { "ContinentalIdMap", ContinentalIdMap },
        };


        var groups = fs.FindAll().GroupBy(x => x.Metadata["TextureName"]);

        foreach (var group in groups)
        {
            var rt = maps[group.Key];
            var format = rt.format switch
            {
                RenderTextureFormat.RFloat => TextureFormat.RFloat,
                RenderTextureFormat.RGFloat => TextureFormat.RGFloat,
                _ => TextureFormat.RGBAFloat,
            };
            var textures = new Texture2D[group.Count()];

            foreach (var file in group)
            {
                using var stream = new MemoryStream();
                fs.Download(file.Id, stream);
                textures[file.Metadata["TextureIndex"]] = new Texture2D(rt.width, rt.height, format, false);
                textures[file.Metadata["TextureIndex"]].LoadRawTextureData(stream.GetBuffer());
                textures[file.Metadata["TextureIndex"]].Apply();
            }

            rt.SetTexture(textures);
        }
    }

    //TODO: make this async
    public static void Save()
    {
        using var db = new LiteDatabase($@"{Application.persistentDataPath}\Environment.db");
        var fs = db.GetStorage<string>();
        var maps = new Dictionary<string, RenderTexture>()
        {
            { "WaterMap", WaterMap },
            { "WaterSourceMap", WaterSourceMap },
            { "LandHeightMap", LandHeightMap },
            { "PlateThicknessMaps", PlateThicknessMaps },
            { "ContinentalIdMap", ContinentalIdMap },
        };

        foreach (var map in maps)
        {
            var name = map.Key;
            var rt = map.Value;
            var i = 0;

            foreach (var tex in rt.CachedTextures(updateCache: true))
            {
                var stream = new MemoryStream(tex.GetRawTextureData());
                fs.Upload($"$/{name}/{i}", $"{i}", stream);
                fs.SetMetadata($"$/{name}/{i}", new BsonDocument(
                    new Dictionary<string, BsonValue> {
                        { "TextureName", new BsonValue(name) },
                        { "TextureIndex", new BsonValue(i) }
                    }));
                i++;
            }
        }
    }

    private RenderTexture NewTexture(int channels, int layers)
    {
        var format = channels switch
        {
            1 => RenderTextureFormat.RFloat,
            2 => RenderTextureFormat.RGFloat,
            _ => RenderTextureFormat.ARGBFloat
        };
        return new RenderTexture(512, 512, 0, format, 0).ResetTexture(layers).Initialize();
    }



    // ---------------

    public EnvironmentMap Load(string planet, EnvironmentMapType mapType)
    {
        using var db = new LiteDatabase($@"{Application.persistentDataPath}\Environment.db");
        var fs = db.GetStorage<string>();

        //this doesn't account for planet and can probably be made more direct
        var map = new EnvironmentMap(mapType);
        var group = fs.FindAll().Where(x => x.Metadata["TextureName"] == map.Name);
        var textures = new Texture2D[group.Count()];

        foreach (var file in group)
        {
            using var stream = new MemoryStream();
            fs.Download(file.Id, stream);
            var tex = new Texture2D(map.RenderTexture.width, map.RenderTexture.height, map.MetaData.TextureFormat, false);
            tex.LoadRawTextureData(stream.GetBuffer());
            tex.Apply();
            textures[file.Metadata["TextureIndex"]] = tex;
        }

        map.SetTextures(textures);
        return map;
    }

    public void Save(string planet, EnvironmentMap map)
    {
        using var db = new LiteDatabase($@"{Application.persistentDataPath}\Environment.db");
        var fs = db.GetStorage<string>();

        var i = 0;
        foreach (var tex in map.CachedTextures)
        {
            var stream = new MemoryStream(tex.GetRawTextureData());
            fs.Upload($"$/{planet}/{map.Name}/{i}", $"{i}", stream);
            fs.SetMetadata($"$/{map.Name}/{i}", new BsonDocument(
                new Dictionary<string, BsonValue> {
                    { "TextureName", new BsonValue(map.Name) },
                    { "TextureIndex", new BsonValue(i) }
                }));
            i++;
        }
    }
}


public class EnvironmentMapMetaDataAttribute: Attribute
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
    public TextureFormat TextureFormat => Channels  switch
    {
        1 => TextureFormat.RFloat,
        2 => TextureFormat.RGFloat,
        _ => TextureFormat.RGBAFloat,
    };

    public EnvironmentMapMetaDataAttribute(string name, int channels = 1, int layers = 6)
    {
        Name = name;
        Channels = channels;
        Layers = layers;
    }
}

public static class EnvironmentMapTypeExtensions
{
    public static EnvironmentMapMetaDataAttribute MetaData(this EnvironmentMapType type)
    {
        var enumType = typeof(EnvironmentMapType);
        var memberInfos = enumType.GetMember(type.ToString());
        var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
        return (EnvironmentMapMetaDataAttribute)enumValueMemberInfo.GetCustomAttributes(typeof(EnvironmentMapMetaDataAttribute), false)[0];
    }
}