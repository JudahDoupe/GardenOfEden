public class WaterData
{
    public string PlanetName { get; }
    public EnvironmentMap WaterMap {get; }
    public EnvironmentMap WaterSourceMap {get; }
    public EnvironmentMap LandHeightMap { get; }

    public bool NeedsRegeneration { get; set; } = false;

    public WaterData(string planetName,
                     EnvironmentMap landHeightMap)
    {
        PlanetName = planetName;
        WaterMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "WaterMap", channels: 4));
        WaterSourceMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "WaterSourceMap"));
        LandHeightMap = landHeightMap;
    }
    public WaterData(WaterDbData dbData,
                     EnvironmentMap waterMap,
                     EnvironmentMap waterSourceMap,
                     EnvironmentMap landHeightMap)
    {
        PlanetName = dbData.PlanetName;
        WaterMap = waterMap;
        WaterSourceMap = waterSourceMap;
        LandHeightMap = landHeightMap;
    }

    public WaterDbData ToDbData() => new WaterDbData
    {
        PlanetName = PlanetName,
        WaterMap = WaterMap.ToDbData(),
        WaterSourceMap = WaterSourceMap.ToDbData(),
        LandHeightMap = LandHeightMap.ToDbData(),
    };
}