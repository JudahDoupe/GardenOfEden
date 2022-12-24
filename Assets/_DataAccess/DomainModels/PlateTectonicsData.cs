using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlateTectonicsData
{
    public PlateTectonicsData(string planetName)
    {
        PlanetName = planetName;
        MantleHeight = 900;
        Plates = new List<PlateData> { new(1.0001f, 0, Random.rotation) };
        LandHeightMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "LandHeightMap"));
        ContinentalIdMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "ContinentalIdMap"));
        VisualizedContinentalIdMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "VisualizedContinentalIdMap"));
        PlateThicknessMaps = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "PlateThicknessMaps"));
        TmpPlateThicknessMaps = new EnvironmentMap(planetName, "TmpPlateThicknessMaps");
    }

    public PlateTectonicsData(PlateTectonicsDbData dbData,
                              EnvironmentMap landHeightMap,
                              EnvironmentMap continentalIdMap,
                              EnvironmentMap plateThicknessMaps)
    {
        PlanetName = dbData.PlanetName;
        MantleHeight = dbData.MantleHeight;
        Plates = dbData.Plates.Select(plateData => new PlateData(plateData)).ToList();
        LandHeightMap = landHeightMap;
        ContinentalIdMap = continentalIdMap;
        VisualizedContinentalIdMap = new EnvironmentMap(PlanetName, "VisualizedContinentalIdMap", ContinentalIdMap.Layers, ContinentalIdMap.Channels);
        PlateThicknessMaps = plateThicknessMaps;
        TmpPlateThicknessMaps = new EnvironmentMap(PlanetName, "TmpPlateThicknessMaps", PlateThicknessMaps.Layers, PlateThicknessMaps.Channels);
        _tools = dbData.Tools?.ToDictionary(x => x.Name, x => new ToolData(x)) ?? new Dictionary<string, ToolData>();
    }

    public string PlanetName { get; }
    public float MantleHeight { get; set; }
    public List<PlateData> Plates { get; set; }
    public EnvironmentMap LandHeightMap { get; }
    public EnvironmentMap ContinentalIdMap { get; }
    public EnvironmentMap VisualizedContinentalIdMap { get; }
    public EnvironmentMap PlateThicknessMaps { get; }
    public EnvironmentMap TmpPlateThicknessMaps { get; }

    private Dictionary<string, ToolData> _tools { get; }
    public ToolData GetTool(string name) => _tools.ContainsKey(name) ? _tools[name] : _tools[name] = new ToolData(name);

    public PlateData GetPlate(float id) => Plates.First(x => Math.Abs(x.Id - id) < float.Epsilon);
    public PlateData AddPlate() => AddPlate(Plates.Max(x => x.Id) + 1f);

    public PlateData AddPlate(float id)
    {
        var plate = new PlateData(id, Plates.Count);

        var oldLayerCount = Plates.Count * 6;
        TmpPlateThicknessMaps.Layers = oldLayerCount;
        Graphics.CopyTexture(PlateThicknessMaps.RenderTexture, TmpPlateThicknessMaps.RenderTexture);

        Plates.Add(plate);

        var newLayerCount = Plates.Count * 6;
        PlateThicknessMaps.Layers = newLayerCount;
        for (var i = 0; i < oldLayerCount; i++)
            Graphics.CopyTexture(TmpPlateThicknessMaps.RenderTexture, i, PlateThicknessMaps.RenderTexture, i);

        return plate;
    }

    public void RemovePlate(float id)
    {
        var plate = GetPlate(id);
        if (plate == null) return;

        var oldLayerCount = Plates.Count * 6;
        TmpPlateThicknessMaps.Layers = oldLayerCount;
        Graphics.CopyTexture(PlateThicknessMaps.RenderTexture, TmpPlateThicknessMaps.RenderTexture);

        Plates.Remove(plate);

        var newLayerCount = Plates.Count * 6;
        PlateThicknessMaps.Layers = newLayerCount;
        foreach (var p in Plates)
        {
            var newIdx = Plates.IndexOf(p);
            for (var i = 0; i < 6; i++)
                Graphics.CopyTexture(TmpPlateThicknessMaps.RenderTexture, p.Idx * 6 + i, PlateThicknessMaps.RenderTexture, newIdx * 6 + i);
            p.Idx = newIdx;
        }
    }

    public PlateTectonicsDbData ToDbData()
        => new()
        {
            PlanetName = PlanetName,
            MantleHeight = MantleHeight,
            Plates = Plates.Select(x => x.ToDbData()).ToArray(),
            LandHeightMap = LandHeightMap.ToDbData(),
            ContinentalIdMap = ContinentalIdMap.ToDbData(),
            PlateThicknessMaps = PlateThicknessMaps.ToDbData(),
            Tools = _tools.Values.Select(x => x.ToDbData()).ToArray()
        };
}

[Serializable]
public class PlateTectonicsDbData
{
    public string PlanetName;
    public float MantleHeight;
    public PlateDbData[] Plates;
    public EnvironmentMapDbData LandHeightMap;
    public EnvironmentMapDbData ContinentalIdMap;
    public EnvironmentMapDbData PlateThicknessMaps;
    public ToolsDbData[] Tools;
}