using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class PlayerDataStore
{
    private static PlayerData _cache;
    private static readonly string Path = $"{Application.persistentDataPath}/Player.data";
    
    public static async Task<PlayerData> GetOrCreate()
    {
        if (_cache != null) 
            return _cache;
        
        var dbData = await LoadDataAsync<PlayerDbData>(Path);
        if (dbData != null)
        {
            _cache = new PlayerData(dbData);
        }
        else
        {
            _cache = new PlayerData(new PlayerDbData
            {
                CurrentPlanetName = null,
                PlanetNames = Array.Empty<string>(),
            });
            await Update(_cache);
        }
        return _cache;
    }
    public static async Task Update(PlayerData data)
    {
        await SaveDataAsync(Path, data.ToDbData());
    }
    
    private static async Task SaveDataAsync<T>(string path, T dbData) => await Task.Run(() => File.WriteAllText(path,  JsonUtility.ToJson(dbData)));
    private static async Task<T> LoadDataAsync<T>(string path) => JsonUtility.FromJson<T>(File.Exists(path) ? await File.ReadAllTextAsync(path) : "");

}