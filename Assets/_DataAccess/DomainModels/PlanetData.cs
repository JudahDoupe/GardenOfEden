public class PlanetData
{
    public string PlanetName { get; set; }
    public PlateTectonicsData PlateTectonics { get; set; }
    public WaterData Water { get; set; }

    public PlanetData(string planetName)
    {
        PlanetName = planetName;
        PlateTectonics = SimulationDataStore.GetOrCreatePlateTectonics(planetName);
        Water = SimulationDataStore.GetOrCreateWater(planetName);
    }
}