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
    public float Radius = 0.5f;
    public float3 Center = new float3(0,0,0);
}

public class SphereColliderComponentBaker : Baker<SphereColliderComponent>
{
    public override void Bake(SphereColliderComponent authoring)
    {
        AddComponent(new SphereCollider
        {
            Center = authoring.Center,
            Radius = authoring.Radius,
        });
    }
}