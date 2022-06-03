using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public static class EnvironmentMapDataStore
{
    private static readonly Dictionary<(string, string), EnvironmentMap> Cache = new Dictionary<(string, string), EnvironmentMap>();

    public static EnvironmentMap GetOrCreate(EnvironmentMapDbData dbData)
    {
        if (Cache.TryGetValue((dbData.PlanetName, dbData.MapName), out var map))
        {
            return map;
        }

        map = Create(dbData);
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
            var index = int.Parse(Path.GetFileNameWithoutExtension(filePath));
            textures[index] = new Texture2D(map.RenderTexture.width, map.RenderTexture.height, map.TextureFormat, false);
            textures[index].LoadRawTextureData(data);
            textures[index].Apply();
        }

        map.SetTextures(textures);
        return map;
    }

    public static EnvironmentMap Create(EnvironmentMapDbData dbData)
    {
        return Cache[(dbData.PlanetName, dbData.MapName)] = new EnvironmentMap(dbData);
    }

    public static void Update(EnvironmentMap map)
    {
        map.RefreshCache(() =>
        {
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