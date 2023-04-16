using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct CapsuleCollider : IComponentData
{
    public float Bounciness;
    public float Radius;
    public float Length;
    public float3 Start;
    public float3 End;
}

public class CapsuleColliderComponent : MonoBehaviour
{
    public float Bounciness = 0.5f;
    public float Radius = 0.5f;
    public float Length = 0.5f;
    public float3 Start = new(0, 0, 0);
    public float3 End = new(0, 0, 0);
}

public class CapsuleColliderComponentBaker : Baker<CapsuleColliderComponent>
{
    public override void Bake(CapsuleColliderComponent authoring)
    {
        AddComponent(new CapsuleCollider
        {
            Bounciness = authoring.Bounciness,
            Radius = authoring.Radius,
            Length = authoring.Length,
            Start = authoring.Start,
            End = authoring.End
        });
        AddComponent<CollisionResponse>();
    }
}