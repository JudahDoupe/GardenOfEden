using Unity.Entities;
using UnityEngine;

public struct Size : IComponentData
{
    public float NodeRadius;
    public float InternodeLength;
}

public class SizeComponent : MonoBehaviour
{
    public float NodeRadius;
    public float InternodeLength;
}

public class SizeComponentBaker : Baker<SizeComponent>
{
    public override void Bake(SizeComponent authoring)
    {
        AddComponent(new Size
        {
            NodeRadius = authoring.NodeRadius,
            InternodeLength = authoring.InternodeLength,
        });
    }
}