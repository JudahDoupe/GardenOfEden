public class PlanetData
{
    public PlanetData(string planetName)
    {
        PlanetName = planetName;
        PlateTectonics = SimulationDataStore.CreatePlateTectonics(planetName);
        Water = SimulationDataStore.CreateWater(planetName);
    }

    public PlanetData(PlanetDbData dbData)
    {
        PlanetName = dbData.PlanetName;
        PlateTectonics = SimulationDataStore.GetOrCreatePlateTectonics(dbData.PlanetName);
        Water = SimulationDataStore.GetOrCreateWater(dbData.PlanetName);
    }

    public string PlanetName { get; }
    public PlateTectonicsData PlateTectonics { get; }
    public WaterData Water { get; }
}