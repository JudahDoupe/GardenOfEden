using System;
using Unity.Mathematics;
using UnityEngine;

public class PlateData
{
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

    public float Id { get; set; }
    public int Idx { get; set; }
    public Quaternion Rotation { get; set; }
    public Quaternion Velocity { get; set; }
    public Quaternion TargetVelocity { get; set; }
    public float TravelDistance => Quaternion.Angle(Quaternion.identity, Velocity) * (math.PI / 180) * Coordinate.PlanetRadius;
    public bool IsInMotion => Quaternion.Angle(Velocity, Quaternion.identity) > 0.0001f;

    public PlateGpuData ToGpuData() => new() { Id = Id, Idx = Idx, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]), TravelDistance = TravelDistance };
    public PlateDbData ToDbData() => new() { Id = Id, Idx = Idx, Rotation = new[] { Rotation[0], Rotation[1], Rotation[2], Rotation[3] } };
}

[Serializable]
public class PlateDbData
{
    public float Id;
    public int Idx;
    public float[] Rotation;
}

public struct PlateGpuData
{
    public float Id;
    public int Idx;
    public float4 Rotation;
    public float TravelDistance;
}