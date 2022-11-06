using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections;

public static class EnvironmentMapDataStore
{
    private static readonly Dictionary<(string, string), EnvironmentMap> Cache = new Dictionary<(string, string), EnvironmentMap>();

    public static async Task<EnvironmentMap> GetOrCreate(EnvironmentMapDbData dbData)
        => Cache.TryGetValue((dbData.PlanetName, dbData.MapName), out var map)
            ? map
            : Cache[(dbData.PlanetName, dbData.MapName)] = await LoadDataAsync(dbData);

    public static EnvironmentMap Create(EnvironmentMapDbData dbData) 
        => Cache[(dbData.PlanetName, dbData.MapName)] = new EnvironmentMap(dbData);

    public static async Task Update(EnvironmentMap map) 
        => await SaveDataAsync(map);

    #region File IO

    private static async Task SaveDataAsync(EnvironmentMap map)
    {

        var refreshTaskSource = new TaskCompletionSource<List<(int, byte[])>>();

        map.RefreshCache(() =>
        {
            Planet.Instance.StartCoroutine(ReadTextureData(map, refreshTaskSource));
        });

        var folderPath = $"{Application.persistentDataPath}/{map.PlanetName}/{map.Name}";
        var rawData = await refreshTaskSource.Task;
        await Task.Run(() => {
            var dir = Directory.CreateDirectory(folderPath);
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }
            foreach(var (i, data) in rawData)
            {
                var filePath = $"{folderPath}/{i}.tex";
                File.WriteAllBytes(filePath, data);
            }
        });
    }

    private static IEnumerator ReadTextureData(EnvironmentMap map, TaskCompletionSource<List<(int, byte[])>> refreshTaskSource)
    {
        var rawData = new List<(int, byte[])>();
        foreach (var (tex, i) in map.CachedTextures.WithIndex())
        {
            rawData.Add((i, tex.GetRawTextureData()));
            yield return new WaitForEndOfFrame();
        }
        refreshTaskSource.TrySetResult(rawData);
    }

    private static async Task<EnvironmentMap> LoadDataAsync(EnvironmentMapDbData dbData)
    {
        var map = Create(dbData);
        var folderPath = $"{Application.persistentDataPath}/{dbData.PlanetName}/{dbData.MapName}";
        Directory.CreateDirectory(folderPath);
        var files = Directory.GetFiles(folderPath);

        if (!files.Any())
        {
            return map;
        }

        var rawData = new List<(int,byte[])>();
        await Task.Run(() => {
            foreach (var filePath in files)
            {
                var index = int.Parse(Path.GetFileNameWithoutExtension(filePath));
                var data = File.ReadAllBytes(filePath);
                rawData.Add((index, data));
            }
        });

        var textures = new Texture2D[files.Length];
        foreach (var (index, data) in rawData)
        {
            textures[index] = new Texture2D(map.RenderTexture.width, map.RenderTexture.height, map.TextureFormat, false);
            textures[index].LoadRawTextureData(data);
            textures[index].Apply();
        }

        map.SetTextures(textures);
        return map;
    }

    #endregion
}