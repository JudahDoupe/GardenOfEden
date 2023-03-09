using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public struct PrimaryGrowth : IComponentData
{
    public float Density;
    public float NodeRadius;
    public float InternodeLength;
}

public class PrimaryGrowthComponent : MonoBehaviour
{
    public float Density;
    public float NodeRadius;
    public float InternodeLength;
}

public class PrimaryGrowthComponentBaker : Baker<PrimaryGrowthComponent>
{
    public override void Bake(PrimaryGrowthComponent authoring)
    {
        AddComponent(new PrimaryGrowth
        {
            Density = authoring.Density,
            NodeRadius = authoring.NodeRadius,
            InternodeLength = authoring.InternodeLength,
        });
    }
}