using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class PlanetDataStore
{
    public static async Task<PlanetData> GetOrCreate(string planetName)
    {
        var folderPath = $"{Application.persistentDataPath}/{planetName}";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        return new PlanetData(new PlanetDbData{ PlanetName = planetName },
            await SimulationDataStore.GetOrCreatePlateTectonics(planetName),
            await SimulationDataStore.GetOrCreateWater(planetName));
    }
    public static async Task<PlanetData> Create(string planetName)
    {
        var folderPath = $"{Application.persistentDataPath}/{planetName}";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        return new PlanetData(new PlanetDbData{ PlanetName = planetName },
            await SimulationDataStore.CreatePlateTectonics(planetName),
            await SimulationDataStore.CreateWater(planetName));
    }
    public static async Task Update(PlanetData data)
    {
        await SimulationDataStore.UpdatePlateTectonics(data.PlateTectonics);
        await SimulationDataStore.UpdateWater(data.Water);
    }
    public static void Delete(string planetName)
    {
        var folderPath = $"{Application.persistentDataPath}/{planetName}";
        Directory.Delete(folderPath, true);
    }
}