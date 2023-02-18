using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SphereCollider : IComponentData
{
    public float Radius;
    public float3 Center;
}

public class SphereColliderComponent : MonoBehaviour
{
}

public class SphereColliderComponentBaker : Baker<SphereColliderComponent>
{
    public override void Bake(SphereColliderComponent authoring)
    {
        AddComponent(new SphereCollider());
    }
}