using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PhysicsBody : IComponentData
{
    public float Mass;
    
    // All in world space
    public float PrevTimeStep;
    public float3 PrevPosition;
    public float3 Position;
    public float3 Acceleration;
    public float3 Displacement => Position - PrevPosition;
    public float3 Velocity => Displacement / PrevTimeStep;
    public void AddAcceleration(float3 acceleration) => Acceleration += acceleration;
    public void AddForce(float3 force) => Acceleration += force * Mass;
    public void AddVelocity(float3 velocity) => PrevPosition -= velocity * PrevTimeStep;
    public void SetVelocity(float3 velocity) => PrevPosition = Position - velocity * PrevTimeStep;
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
        var position = authoring.transform.position;
        AddComponent(e, new PhysicsBody
        {
            Mass = authoring.Mass,
            Position = position,
            PrevPosition = position,
            Acceleration = float3.zero,
            PrevTimeStep = 1,
        });
    }
}