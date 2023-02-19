using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Physics : IComponentData
{
    public float Mass;
    public float3 Force;
    public float3 Velocity;
}

public class PhysicsComponent : MonoBehaviour
{
    public float Mass;
}

public class PhysicsComponentBaker : Baker<PhysicsComponent>
{
    public override void Bake(PhysicsComponent authoring)
    {
        AddComponent(new Physics
        {
            Mass = authoring.Mass,
        });
    }
}