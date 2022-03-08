using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteDB;
using System.IO;

public static class EnvironmentMapDataStore
{
    public static EnvironmentMap WaterSourceMap { get; private set; }
    public static EnvironmentMap WaterMap { get; private set; }
    public static EnvironmentMap LandHeightMap { get; private set; }
    public static EnvironmentMap PlateThicknessMaps { get; private set; }
    public static EnvironmentMap ContinentalIdMap { get; private set; }

    private static string ConnectionString => $@"{Application.persistentDataPath}\Environment.db";

    public static void Load(string planetName)
    {
        WaterSourceMap = Load(planetName, EnvironmentMapType.WaterSourceMap);
        WaterMap = Load(planetName, EnvironmentMapType.WaterMap);
        LandHeightMap = Load(planetName, EnvironmentMapType.LandHeightMap);
        PlateThicknessMaps = Load(planetName, EnvironmentMapType.PlateThicknessMaps);
        ContinentalIdMap = Load(planetName, EnvironmentMapType.ContinentalIdMap);
    }
    private static EnvironmentMap Load(string planetName, EnvironmentMapType mapType)
    {
        using var db = new LiteDatabase(ConnectionString);
        var fs = db.GetStorage<string>();

        var map = new EnvironmentMap(mapType);
        var files = fs.FindAll().Where(x => x.Metadata["Map"] == map.Name && x.Metadata["Planet"] == planetName).ToArray();

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

    public static void Save(string planetName)
    {
        Save(planetName, WaterSourceMap);
        Save(planetName, WaterMap);
        Save(planetName, LandHeightMap);
        Save(planetName, PlateThicknessMaps);
        Save(planetName, ContinentalIdMap);
    }
    public static void Save(string planetName, EnvironmentMap map)
    {
        using var db = new LiteDatabase(ConnectionString);
        var fs = db.GetStorage<string>();

        foreach (var (tex, i) in map.CachedTextures.WithIndex())
        {
            var path = $"$/{planetName}/{map.Name}/{i}";
            var stream = new MemoryStream(tex.GetRawTextureData());
            var metaData = new Dictionary<string, BsonValue>
            {
                { "Planet", new BsonValue(planetName) },
                { "Map", new BsonValue(map.Name) },
                { "Index", new BsonValue(i) },
            };

            fs.Upload(path, $"{map.Name}{i}", stream);
            fs.SetMetadata(path, new BsonDocument(metaData));
        }
    }
}
