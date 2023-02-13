using Unity.Entities;
using UnityEngine;

public struct PrimaryGrowthTarget : IComponentData
{
    public float NodeSize;
    public float InternodeLength;
}

public struct Size : IComponentData
{
    public float NodeSize;
    public float InternodeLength;
}

public class PrimaryGrowthComponent : MonoBehaviour
{
    public float NodeSize;
    public float InternodeLength;
}

public class PrimaryGrowthComponentBaker : Baker<PrimaryGrowthComponent>
{
    public override void Bake(PrimaryGrowthComponent authoring)
    {
        AddComponent(new PrimaryGrowthTarget
        {
            NodeSize = authoring.NodeSize,
            InternodeLength = authoring.InternodeLength,
        });
        AddComponent(new Size
        {
            NodeSize = 0,
            InternodeLength = 0,
        });
    }
}