using LiteDB;
using UnityEngine;

public static class PlanetDataStore
{
    public static PlanetData GetOrCreate(string planetName)
    {
        var plateTectonics = SimulationDataStore.GetOrCreatePlateTectonics(planetName);
        var water = SimulationDataStore.GetOrCreateWater(planetName);
        return new PlanetData(planetName, plateTectonics, water);
    }
    public static PlanetData Create(string planetName)
    {
        var plateTectonics = SimulationDataStore.CreatePlateTectonics(planetName);
        var water = SimulationDataStore.CreateWater(planetName);
        return new PlanetData(planetName, plateTectonics, water);
    }
    public static void Update(PlanetData data)
    {
        Debug.Log("Saving Planet");
        SimulationDataStore.UpdatePlateTectonics(data.PlateTectonics);
        SimulationDataStore.UpdateWater(data.Water);
    }
}