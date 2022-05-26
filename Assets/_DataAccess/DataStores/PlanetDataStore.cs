using LiteDB;
using UnityEngine;

public static class PlanetDataStore
{
    public static PlanetData GetOrCreate(string planetName)
    {
        return new PlanetData
        {
            PlanetName = planetName,
            PlateTectonics = SimulationDataStore.GetOrCreatePlateTectonics(planetName),
            Water = SimulationDataStore.GetOrCreateWater(planetName),
        };
    }
    public static PlanetData Create(string planetName)
    {
        return new PlanetData
        {
            PlanetName = planetName,
            PlateTectonics = SimulationDataStore.CreatePlateTectonics(planetName),
            Water = SimulationDataStore.CreateWater(planetName),
        };
    }
    public static void Update(PlanetData data)
    {
        Debug.Log("Saving Planet");
        SimulationDataStore.UpdatePlateTectonics(data.PlateTectonics);
        SimulationDataStore.UpdateWater(data.Water);
    }
}