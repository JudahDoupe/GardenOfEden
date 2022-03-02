using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteDB;
using System.IO;

public class EnvironmentMapDataStore : MonoBehaviour
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

    protected static string ConnectionString => $@"{Application.persistentDataPath}\Environment.db";

    public EnvironmentMap Load(string planet, EnvironmentMapType mapType)
    {
        using var db = new LiteDatabase(ConnectionString);
        var fs = db.GetStorage<string>();

        var map = new EnvironmentMap(mapType);
        var files = fs.FindAll().Where(x => x.Metadata["Map"] == map.Name && x.Metadata["Planet"] == planet).ToArray();

        if (!files.Any()) return map;

        var textures = new Texture2D[files.Length];
            
        foreach (var file in files)
        {
            using var stream = new MemoryStream();
            var i = file.Metadata["Index"];

            fs.Download(file.Id, stream);
            textures[i] = new Texture2D(map.RenderTexture.width, map.RenderTexture.height, map.MetaData.TextureFormat, false);
            textures[i].LoadRawTextureData(stream.GetBuffer());
            textures[i].Apply();
        }
            
        map.SetTextures(textures);

        return map;
    }

    public void Save(string planet, EnvironmentMap map)
    {
        using var db = new LiteDatabase(ConnectionString);
        var fs = db.GetStorage<string>();

        foreach (var (tex, i) in map.CachedTextures.WithIndex())
        {
            var path = $"$/{planet}/{map.Name}/{i}";
            var stream = new MemoryStream(tex.GetRawTextureData());
            var metaData = new Dictionary<string, BsonValue>
            {
                { "Planet", new BsonValue(planet) },
                { "Map", new BsonValue(map.Name) },
                { "Index", new BsonValue(i) },
            };

            fs.Upload(path, $"{map.Name}{i}", stream);
            fs.SetMetadata(path, new BsonDocument(metaData));
        }
    }
}
