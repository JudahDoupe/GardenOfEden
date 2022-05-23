using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public static class EnvironmentMapDataStore
{
    private static Dictionary<(string, string), EnvironmentMap> Cache = new Dictionary<(string, string), EnvironmentMap>();

    public static EnvironmentMap GetOrCreate(string planetName, string mapName, int layers = 6, int channels = 1)
    {
        if (!Cache.TryGetValue((planetName, mapName), out var map))
        {
            Cache[(planetName, mapName)] = map = new EnvironmentMap(planetName, mapName, layers, channels);
        }
        var folderPath = $"{Application.persistentDataPath}/{planetName}/{mapName}";
        Directory.CreateDirectory(folderPath);
        var files = Directory.GetFiles(folderPath);

        if (!files.Any())
        {
            return map;
        }

        var textures = new Texture2D[files.Length];
        foreach (var filePath in files)
        {
            var data = File.ReadAllBytes(filePath);
            var index = Int32.Parse(Path.GetFileNameWithoutExtension(filePath));
            textures[index] = new Texture2D(map.RenderTexture.width, map.RenderTexture.height, map.TextureFormat, false);
            textures[index].LoadRawTextureData(data);
            textures[index].Apply();
        }

        map.SetTextures(textures);
        return map;
    }

    public static EnvironmentMap GetOrCreate(EnvironmentMapDbData dbData)
    {
        if (!Cache.TryGetValue((dbData.PlanetName, dbData.MapName), out var map))
        {
            Cache[(dbData.PlanetName, dbData.MapName)] = map = new EnvironmentMap(dbData);
        }
        var folderPath = $"{Application.persistentDataPath}/{dbData.PlanetName}/{dbData.MapName}";
        Directory.CreateDirectory(folderPath);
        var files = Directory.GetFiles(folderPath);

        if (!files.Any())
        {
            return map;
        }

        var textures = new Texture2D[files.Length];
        foreach (var filePath in files)
        {
            var data = File.ReadAllBytes(filePath);
            var index = Int32.Parse(Path.GetFileNameWithoutExtension(filePath));
            textures[index] = new Texture2D(map.RenderTexture.width, map.RenderTexture.height, map.TextureFormat, false);
            textures[index].LoadRawTextureData(data);
            textures[index].Apply();
        }

        map.SetTextures(textures);
        return map;
    }

    public static void Update(EnvironmentMap map)
    {
        map.RefreshCache(() =>
        {
            Debug.Log($"Saving {map.PlanetName}'s {map.Name}");
            var folderPath = $"{Application.persistentDataPath}/{map.PlanetName}/{map.Name}";
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