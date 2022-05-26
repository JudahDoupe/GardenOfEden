using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class PlateTectonicsData
{
    public string PlanetName { get; }
    public List<PlateData> Plates { get; }
    public EnvironmentMap LandHeightMap { get; }
    public EnvironmentMap ContinentalIdMap { get; }
    public EnvironmentMap PlateThicknessMaps { get; }
    public EnvironmentMap TmpPlateThicknessMaps { get; }

    public bool NeedsRegeneration { get; set; } = false;

    public PlateTectonicsData(string planetName)
    {
        PlanetName = planetName;
        Plates = new List<PlateData> { new PlateData(0, 0) };
        LandHeightMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "LandHeightMap"));
        ContinentalIdMap = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "ContinentalIdMap"));
        PlateThicknessMaps = EnvironmentMapDataStore.Create(new EnvironmentMapDbData(planetName, "PlateThicknessMaps"));
        TmpPlateThicknessMaps = new EnvironmentMap(planetName, "TmpPlateThicknessMaps");
    }
    public PlateTectonicsData(PlateTectonicsDbData dbData)
    {
        PlanetName = dbData.PlanetName;
        Plates = dbData.Plates.Select(plateData => new PlateData(plateData)).ToList();
        LandHeightMap = EnvironmentMapDataStore.GetOrCreate(dbData.LandHeightMap);
        ContinentalIdMap = EnvironmentMapDataStore.GetOrCreate(dbData.ContinentalIdMap);
        PlateThicknessMaps = EnvironmentMapDataStore.GetOrCreate(dbData.PlateThicknessMaps);
        TmpPlateThicknessMaps = new EnvironmentMap(PlanetName, "TmpPlateThicknessMaps", PlateThicknessMaps.Layers, PlateThicknessMaps.Channels);
    }

    public PlateTectonicsDbData ToDbData() => new PlateTectonicsDbData
    {
        PlanetName = PlanetName,
        Plates = Plates.Select(x => x.ToDbData()).ToArray(),
        LandHeightMap = LandHeightMap.ToDbData(),
        ContinentalIdMap = ContinentalIdMap.ToDbData(),
        PlateThicknessMaps = PlateThicknessMaps.ToDbData(),
    };
}

public class PlateData
{
    public bool NeedsRegeneration { get; set; }
    public float Id { get; set; }
    public int Idx { get; set; }
    public Quaternion Rotation { get; set; }
    public Quaternion Velocity { get; set; }
    public Quaternion TargetVelocity { get; set; }
    public Vector3 Center => Rotation * Vector3.forward * 1000;
    public bool IsStopped => Quaternion.Angle(Velocity, Quaternion.identity) < 0.001f;
    public bool IsAligned => Quaternion.Angle(Rotation, Quaternion.identity) < 0.001f;

    public PlateData(float id, int idx, Quaternion? rotation = null)
    {
        Id = id;
        Idx = idx;
        Rotation = rotation ?? Quaternion.identity;
        Velocity = Quaternion.identity;
        TargetVelocity = Quaternion.identity;
    }
    public PlateData(PlateDbData dbData)
    {
        Id = dbData.Id;
        Idx = dbData.Idx;
        Rotation = dbData.Rotation != null 
            ? new Quaternion(dbData.Rotation[0], dbData.Rotation[1], dbData.Rotation[2], dbData.Rotation[3])
            : Quaternion.identity;
        Velocity = Quaternion.identity;
        TargetVelocity = Quaternion.identity;
    }
    
    public PlateGpuData ToGpuData() => new PlateGpuData { Id = Id, Idx = Idx, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]) };
    public PlateDbData ToDbData() => new PlateDbData { Id = Id, Idx = Idx, Rotation = new float[] { Rotation[0], Rotation[1], Rotation[2], Rotation[3] } };

}
