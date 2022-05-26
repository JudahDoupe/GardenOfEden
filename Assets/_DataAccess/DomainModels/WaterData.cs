public class WaterData
{
    public string PlanetName { get; set; }
    public EnvironmentMap WaterMap {get; set;}
    public EnvironmentMap WaterSourceMap {get; set;}
    public EnvironmentMap LandHeightMap { get; set;}

    public bool NeedsRegeneration { get; set; } = false;

    public WaterData(string planetName)
    {
        PlanetName = planetName;
        WaterMap = EnvironmentMapDataStore.Create(planetName, "WaterMap", channels: 4);
        WaterSourceMap = EnvironmentMapDataStore.Create(planetName, "WaterSourceMap");
        LandHeightMap = EnvironmentMapDataStore.Create(planetName, "LandHeightMap");
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