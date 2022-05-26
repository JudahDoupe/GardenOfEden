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
        WaterMap = Create("WaterMap", 4);
        WaterSourceMap = Create("WaterSourceMap");
        LandHeightMap = Create("LandHeightMap");

        EnvironmentMap Create(string name, int chanels = 1) => EnvironmentMapDataStore.Create(new EnvironmentMapDbData
        {
            PlanetName = planetName,
            MapName = name,
            Channels = chanels,
            Layers = 6
        });
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