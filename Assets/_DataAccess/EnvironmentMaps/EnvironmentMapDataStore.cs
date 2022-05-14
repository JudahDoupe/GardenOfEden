using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteDB;
using System.IO;
using System.Threading.Tasks;

public static class EnvironmentMapDataStore
{
    public static bool IsLoaded { get; private set; } = false;
    public static string PlanetName { get; private set; } = "";

    public static EnvironmentMap WaterSourceMap { get; } = new EnvironmentMap(EnvironmentMapType.WaterSourceMap);
    public static EnvironmentMap WaterMap { get; } = new EnvironmentMap(EnvironmentMapType.WaterMap);
    public static EnvironmentMap LandHeightMap { get; } = new EnvironmentMap(EnvironmentMapType.LandHeightMap);
    public static EnvironmentMap PlateThicknessMaps { get; } = new EnvironmentMap(EnvironmentMapType.PlateThicknessMaps);
    public static EnvironmentMap ContinentalIdMap { get; } = new EnvironmentMap(EnvironmentMapType.ContinentalIdMap);

    public static void Load(string planetName)
    {
        PlanetName = planetName;

        Load(WaterSourceMap);
        Load(WaterMap);
        Load(LandHeightMap);
        Load(PlateThicknessMaps);
        Load(ContinentalIdMap);

        Debug.Log($"Planet {PlanetName} Loaded");
        IsLoaded = true;
    }

    private static void Load(EnvironmentMap map)
    {
        var folderPath = $"{Application.persistentDataPath}/{PlanetName}/{map.Name}";
        Directory.CreateDirectory(folderPath);
        var files = Directory.GetFiles(folderPath);

        if (!files.Any())
        {
            map.ResetTexture();
            return;
        }

        var textures = new Texture2D[files.Length];
        foreach (var filePath in files)
        {
            var data = File.ReadAllBytes(filePath);
            var index = Int32.Parse(Path.GetFileNameWithoutExtension(filePath));
            textures[index] = new Texture2D(map.RenderTexture.width, map.RenderTexture.height, map.MetaData.TextureFormat, false);
            textures[index].LoadRawTextureData(data);
            textures[index].Apply();
        }

        map.SetTextures(textures);
    }

    public static void Save()
    {
        Save(WaterSourceMap);
        Save(WaterMap);
        Save(LandHeightMap);
        Save(PlateThicknessMaps);
        Save(ContinentalIdMap);

        Debug.Log($"Planet {PlanetName} Saved");
    }

    private static void Save(EnvironmentMap map)
    {
        map.RefreshCache(() =>
        {
            var folderPath = $"{Application.persistentDataPath}/{PlanetName}/{map.Name}";
            Directory.CreateDirectory(folderPath);
            foreach (var (tex, i) in map.CachedTextures.WithIndex())
            {
                byte[] data = tex.GetRawTextureData();
                var filePath = $"{folderPath}/{i}.tex";
                File.WriteAllBytes(filePath, data);
            }
        });

    }
}