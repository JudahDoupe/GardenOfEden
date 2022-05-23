using LiteDB;
using UnityEngine;

public static class PlanetDataStore
{
    public static PlanetData GetOrCreate(string planetName)
    {
        return new PlanetData(planetName);
    }
    public static void UpdatePlanet(PlanetData data)
    {
        Debug.Log("Saving Planet");
        SimulationDataStore.UpdatePlateTectonics(data.PlateTectonics);
        SimulationDataStore.UpdateWater(data.Water);
    }
}