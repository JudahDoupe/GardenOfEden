using System.IO;
using UnityEngine;

public static class PlanetDataStore
{
    public static PlanetData GetOrCreate(string planetName)
    {
        var folderPath = $"{Application.persistentDataPath}/{planetName}";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        return new PlanetData(new PlanetDbData{PlanetName = planetName});
    }
    public static PlanetData Create(string planetName)
    {
        return new PlanetData(planetName);
    }
    public static void Update(PlanetData data)
    {
        SimulationDataStore.UpdatePlateTectonics(data.PlateTectonics);
        SimulationDataStore.UpdateWater(data.Water);
    }
}