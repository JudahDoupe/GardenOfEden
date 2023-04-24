using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct CollisionResponse : IComponentData
{
    public float3 VelocityAdjustment;
    public float3 PositionAdjustment;
}

public struct SphereCollider : IComponentData
{
    public float Bounciness;
    public float Radius;
    public float3 Center;
}

public class SphereColliderComponent : MonoBehaviour
{    
    public float Bounciness = 0.5f;
    public float Radius = 0.5f;
    public float3 Center = new float3(0,0,0);
}

public class SphereColliderComponentBaker : Baker<SphereColliderComponent>
{
    public override void Bake(SphereColliderComponent authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(e, new SphereCollider
        {
            Center = authoring.Center,
            Radius = authoring.Radius,
            Bounciness = authoring.Bounciness,
        });
        AddComponent<CollisionResponse>(e);
    }
}