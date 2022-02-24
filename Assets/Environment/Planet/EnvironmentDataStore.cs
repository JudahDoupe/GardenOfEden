using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteDB;
using System.IO;
using System;

public class EnvironmentDataStore : MonoBehaviour
{
    public static RenderTexture WaterSourceMap { get; set; }
    public static RenderTexture WaterMap { get; set; }
    public static RenderTexture LandHeightMap { get; set; }
    public static RenderTexture PlateThicknessMaps { get; set; }
    public static RenderTexture TmpPlateThicknessMaps { get; set; }
    public static RenderTexture ContinentalIdMap { get; set; }
    public static RenderTexture TmpContinentalIdMap { get; set; }
    
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

            foreach (var tex in rt.CachedTextures())
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

    private class RenderTextureMetaData : BsonDocument
    {
        public int TextureIndex;
        public string TextureName;
    }
}