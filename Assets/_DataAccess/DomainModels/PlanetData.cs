public class PlanetData
{
    public PlanetData(PlanetDbData dbData,
                      PlateTectonicsData plateTectonics,
                      WaterData waterData)
    {
        PlanetName = dbData.PlanetName;
        PlateTectonics = plateTectonics;
        Water = waterData;
    }

    public string PlanetName { get; }
    public PlateTectonicsData PlateTectonics { get; }
    public WaterData Water { get; }
}