using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class SimulationDataStore
{

    #region PlateTectonics

    private static string PlateTectonicsDataPath(string planetName) => $"{Application.persistentDataPath}/{planetName}/PlateTectonics.data";

    public static PlateTectonicsData GetOrCreatePlateTectonics(string planetName)
    {
        var json = LoadData(PlateTectonicsDataPath(planetName));
        var dbData = JsonUtility.FromJson<PlateTectonicsDbData>(json);

        if (dbData != null)
            return new PlateTectonicsData(dbData);
        else
            return CreatePlateTectonics(planetName);
    }

    public static PlateTectonicsData CreatePlateTectonics(string planetName)
    {
        var data = PateTectonicsGenerator.Generate(planetName);
        UpdatePlateTectonics(data);
        return data;
    }

    public static void UpdatePlateTectonics(PlateTectonicsData data)
    {
        EnvironmentMapDataStore.Update(data.LandHeightMap);
        EnvironmentMapDataStore.Update(data.ContinentalIdMap);
        EnvironmentMapDataStore.Update(data.PlateThicknessMaps);

        var dbData = data.ToDbData();
        var json = JsonUtility.ToJson(dbData);
        SaveData(PlateTectonicsDataPath(data.PlanetName), json);
    }

    #endregion

    #region Water

    private static string WaterDataPath(string planetName) => $"{Application.persistentDataPath}/{planetName}/Water.data";

    public static WaterData GetOrCreateWater(string planetName)
    {
        var json = LoadData(WaterDataPath(planetName));
        var dbData = JsonUtility.FromJson<WaterDbData>(json);

        if (dbData != null)
            return new WaterData(dbData);
        else
            return CreateWater(planetName);
    }

    public static WaterData CreateWater(string planetName)
    {
        var data = new WaterData(planetName) { NeedsRegeneration = true };
        UpdateWater(data);
        return data;
    }

    public static void UpdateWater(WaterData data)
    {
        EnvironmentMapDataStore.Update(data.WaterMap);

        var json = JsonUtility.ToJson(data.ToDbData());
        SaveData(WaterDataPath(data.PlanetName), json);

    }

    #endregion


    private static Task SaveData(string path, string json) => Task.Run(() => File.WriteAllText(path, json));
    private static string LoadData(string path) => File.Exists(path) ? File.ReadAllText(path) : "";
}

