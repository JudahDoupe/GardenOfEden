using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PhysicsBody : IComponentData
{
    public float Mass;
    public float3 Force;
    public float3 Velocity;
}

public class PhysicsComponent : MonoBehaviour
{
    public float Mass = 1;
}

public class PhysicsComponentBaker : Baker<PhysicsComponent>
{
    public override void Bake(PhysicsComponent authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(e, new PhysicsBody
        {
            Mass = authoring.Mass,
        });
    }
}