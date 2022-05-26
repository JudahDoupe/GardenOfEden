using LiteDB;
using UnityEngine;

public static class SimulationDataStore
{
    private static string ConnectionString => $@"{Application.persistentDataPath}\Simulations.db";

    #region PlateTectonics

    public static PlateTectonicsData GetOrCreatePlateTectonics(string planetName)
    {
        using var db = new LiteDatabase(ConnectionString);
        var collection = db.GetCollection<PlateTectonicsDbData>("PlateTectonics");
        collection.EnsureIndex(x => x.PlanetName);
        var dbData = collection.FindOne(x => x.PlanetName.Equals(planetName));

        if (dbData != null)
            return new PlateTectonicsData(dbData);
        else
            return CreatePlateTectonics(planetName);
    }

    public static PlateTectonicsData CreatePlateTectonics(string planetName)
    {
        return new PlateTectonicsData(planetName) { NeedsRegeneration = true };
    }

    public static void UpdatePlateTectonics(PlateTectonicsData data)
    {
        Debug.Log($"Saving {data.PlanetName}'s Plate Tectonics");
        EnvironmentMapDataStore.Update(data.LandHeightMap);
        EnvironmentMapDataStore.Update(data.ContinentalIdMap);
        EnvironmentMapDataStore.Update(data.PlateThicknessMaps);

        using var db = new LiteDatabase(ConnectionString);
        var collection = db.GetCollection<PlateTectonicsDbData>("PlateTectonics");
        collection.EnsureIndex(x => x.PlanetName);
        collection.Insert(data.ToDbData());
    }

    #endregion

    #region Water

    public static WaterData GetOrCreateWater(string planetName)
    {
        using var db = new LiteDatabase(ConnectionString);
        var collection = db.GetCollection<WaterDbData>("Water");
        var dbData = collection.FindOne(x => x.PlanetName.Equals(planetName));

        if (dbData != null)
            return new WaterData(dbData);
        else
            return CreateWater(planetName);
    }

    public static WaterData CreateWater(string planetName)
    {
        return new WaterData(planetName) { NeedsRegeneration = true };
    }

    public static void UpdateWater(WaterData data)
    {
        Debug.Log($"Saving {data.PlanetName}'s Water");
        EnvironmentMapDataStore.Update(data.WaterMap);

        using var db = new LiteDatabase(ConnectionString);
        var collection = db.GetCollection<WaterDbData>("Water");
        collection.Insert(data.ToDbData());
    }

    #endregion
    
}

