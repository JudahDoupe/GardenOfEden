using Unity.Mathematics;
using UnityEngine;

public class Plate
{
    public Plate() {}

    public Plate(PlateData data)
    {
        Id = data.Id;
        Idx = data.Idx;
        Rotation = new Quaternion(data.Rotation.x, data.Rotation.y, data.Rotation.z, data.Rotation.w);
    }

    public float Id;
    public int Idx;
    public Quaternion Rotation;
    public Quaternion Velocity;
    public Quaternion TargetVelocity;
    public Vector3 Center => Rotation * Vector3.forward * (Singleton.Water.SeaLevel + 100);
    public bool IsStopped => Quaternion.Angle(Velocity, Quaternion.identity) < 0.001f;
    public bool IsAligned => Quaternion.Angle(Rotation, Quaternion.identity) < 0.001f;
    public PlateData Serialize() => new PlateData { Id = Id, Idx = Idx, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]) };

}

public struct PlateData
{
    public float Id;
    public int Idx;
    public float4 Rotation;
}
