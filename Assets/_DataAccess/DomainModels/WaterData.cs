public class WaterData
{
    public string PlanetName { get; }
    public EnvironmentMap WaterMap {get; }
    public EnvironmentMap WaterSourceMap {get; }
    public EnvironmentMap LandHeightMap { get; }

    public bool NeedsRegeneration { get; set; } = false;

    public WaterData(string planetName)
    {
        PlanetName = planetName;
        WaterMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "WaterMap", channels: 4));
        WaterSourceMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "WaterSourceMap"));
        LandHeightMap = EnvironmentMapDataStore.GetOrCreate(new EnvironmentMapDbData(planetName, "LandHeightMap"));
    }
    public WaterData(WaterDbData dbData)
    {
        PlanetName = dbData.PlanetName;
        WaterMap = EnvironmentMapDataStore.GetOrCreate(dbData.WaterMap);
        WaterSourceMap = EnvironmentMapDataStore.GetOrCreate(dbData.WaterSourceMap);
        LandHeightMap = EnvironmentMapDataStore.GetOrCreate(dbData.LandHeightMap);
    }

    public WaterDbData ToDbData() => new WaterDbData
    {
        PlanetName = PlanetName,
        WaterMap = WaterMap.ToDbData(),
        WaterSourceMap = WaterSourceMap.ToDbData(),
        LandHeightMap = LandHeightMap.ToDbData(),
    };
}