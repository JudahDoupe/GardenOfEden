public class PlanetData
{
    public string PlanetName { get; set; }
    public PlateTectonicsData PlateTectonics { get; set; }
    public WaterData Water { get; set; }

    public PlanetData(string planetName, PlateTectonicsData plateTectonics, WaterData water)
    {
        PlanetName = planetName;
        PlateTectonics = plateTectonics;
        Water = water;
    }
}