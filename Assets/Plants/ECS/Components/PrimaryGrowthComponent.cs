using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public struct PrimaryGrowth : IComponentData
{
    public float NodeRadius;
    public float InternodeLength;
    public float EnergyToVolumeRatio;
    public float Density;
}

public class PrimaryGrowthComponent : MonoBehaviour
{
    public float NodeRadius;
    public float InternodeLength;
}

public class PrimaryGrowthComponentBaker : Baker<PrimaryGrowthComponent>
{
    public override void Bake(PrimaryGrowthComponent authoring)
    {
        AddComponent(new PrimaryGrowth
        {
            NodeRadius = authoring.NodeRadius,
            InternodeLength = authoring.InternodeLength,
            EnergyToVolumeRatio = 1,
            Density = 1,
        });
    }
}