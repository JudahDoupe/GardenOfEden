using Unity.Mathematics;
using UnityEngine;

public class Plate
{
    public float Id;
    public int Idx;
    public Quaternion Rotation;
    public Quaternion Velocity;
    public Quaternion TargetVelocity;
    public Vector3 Center => Rotation * Vector3.forward * (Singleton.Water.SeaLevel + 100);
    public bool IsStopped => Quaternion.Angle(Velocity, Quaternion.identity) < 0.001f;
    public bool IsAligned => Quaternion.Angle(Rotation, Quaternion.identity) < 0.001f;
    public GpuData ToGpuData() => new GpuData { Id = Id, Idx = Idx, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]) };

    public struct GpuData
    {
        public float Id;
        public int Idx;
        public float4 Rotation;
    }
}
