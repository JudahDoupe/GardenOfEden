using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct PrimaryGrowth : IComponentData
{
    public float NodeRadius;
    public float InternodeLength;
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
        });
    }
}