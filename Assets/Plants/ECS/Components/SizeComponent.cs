using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Size : IComponentData
{
    public float NodeRadius;
    public float InternodeLength;
    public float3 LocalDirection;
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
            InternodeLength = math.max(authoring.InternodeLength,0.001f),
            LocalDirection = authoring.transform.parent.InverseTransformDirection(authoring.transform.forward),
        });
    }
}