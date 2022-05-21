using LiteDB;
using UnityEngine;

public static class SimulationDataStore
{
    private static string ConnectionString => $@"{Application.persistentDataPath}\Simulations.db";

    public static void SavePlateTectonicsSimulation(PlateTectonicsSimulationDto data)
    {
        using var db = new LiteDatabase(ConnectionString);
        var collection = db.GetCollection<PlateTectonicsSimulationDto>("PlateTectonics");
        collection.EnsureIndex(x => x.PlanetName);
        collection.Insert(data);
    }

    public static PlateTectonicsSimulationDto LoadPlateTectonicsSimulation(string planetName)
    {
        using var db = new LiteDatabase(ConnectionString);
        var collection = db.GetCollection<PlateTectonicsSimulationDto>("PlateTectonics");
        collection.EnsureIndex(x => x.PlanetName);
        return collection.FindOne(x => x.PlanetName.Equals(planetName)) ?? new PlateTectonicsSimulationDto
        {
            PlanetName = planetName
        };
    }
}

