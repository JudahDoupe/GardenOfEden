using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class SimulationDataStore
{

    #region PlateTectonics

    private static string PlateTectonicsDataPath(string planetName) => $"{Application.persistentDataPath}/{planetName}/PlateTectonics.data";

    public static async Task<PlateTectonicsData> GetOrCreatePlateTectonics(string planetName)
    {
        var dbData = await LoadDataAsync<PlateTectonicsDbData>(PlateTectonicsDataPath(planetName));

        if (dbData != null)
            return new PlateTectonicsData(dbData);
        else
            return await CreatePlateTectonics(planetName);
    }

    public static async Task<PlateTectonicsData> CreatePlateTectonics(string planetName)
    {
        var data = PateTectonicsGenerator.Generate(planetName);
        await UpdatePlateTectonics(data);
        return data;
    }

    public static async Task UpdatePlateTectonics(PlateTectonicsData data)
    {
        EnvironmentMapDataStore.Update(data.LandHeightMap);
        EnvironmentMapDataStore.Update(data.ContinentalIdMap);
        EnvironmentMapDataStore.Update(data.PlateThicknessMaps);

        var dbData = data.ToDbData();
        var json = JsonUtility.ToJson(dbData);
        await SaveDataAsync(PlateTectonicsDataPath(data.PlanetName), json);
    }

    #endregion

    #region Water

    private static string WaterDataPath(string planetName) => $"{Application.persistentDataPath}/{planetName}/Water.data";

    public static async Task<WaterData> GetOrCreateWater(string planetName)
    {
        var dbData = await LoadDataAsync<WaterDbData>(WaterDataPath(planetName));

        if (dbData != null)
            return new WaterData(dbData);
        else
            return await CreateWater(planetName);
    }

    public static async Task<WaterData> CreateWater(string planetName)
    {
        var data = new WaterData(planetName) { NeedsRegeneration = true };
        await UpdateWater(data);
        return data;
    }

    public static async Task UpdateWater(WaterData data)
    {
        EnvironmentMapDataStore.Update(data.WaterMap);

        await SaveDataAsync(WaterDataPath(data.PlanetName), data.ToDbData());
    }

    #endregion


    private static async Task SaveDataAsync<T>(string path, T dbData) => await Task.Run(() => File.WriteAllText(path,  JsonUtility.ToJson(dbData)));
    private static async Task<T> LoadDataAsync<T>(string path) => JsonUtility.FromJson<T>(File.Exists(path) ? await File.ReadAllTextAsync(path) : "");
}

