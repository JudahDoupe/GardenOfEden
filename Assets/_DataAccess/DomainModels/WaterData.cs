using System;

public class WaterData
{
    public WaterData(string planetName,
                     EnvironmentMap landHeightMap)
    {
        PlanetName = planetName;
        WaterMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData{PlanetName = planetName, MapName = "WaterMap", Channels = 4});
        WaterSourceMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData{ PlanetName = planetName, MapName = "WaterSourceMap"});
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

    public string PlanetName { get; }
    public EnvironmentMap WaterMap { get; }
    public EnvironmentMap WaterSourceMap { get; }
    public EnvironmentMap LandHeightMap { get; }

    public bool NeedsRegeneration { get; set; }

    public WaterDbData ToDbData()
        => new()
        {
            PlanetName = PlanetName,
            WaterMap = WaterMap.ToDbData(),
            WaterSourceMap = WaterSourceMap.ToDbData(),
            LandHeightMap = LandHeightMap.ToDbData()
        };
}

[Serializable]
public class WaterDbData
{
    public string PlanetName;
    public EnvironmentMapDbData WaterMap;
    public EnvironmentMapDbData WaterSourceMap;
    public EnvironmentMapDbData LandHeightMap;
}