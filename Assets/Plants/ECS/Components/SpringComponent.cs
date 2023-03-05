using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public struct Spring : IComponentData
{
    public Entity nodeEntity;
    public Entity nodeParentEntity;
    public float3 nodeParentOffset;

    public float strength;
    public float damping;
}

public class SpringComponent : MonoBehaviour
{
    public float strength;
    public float damping;

    private void OnEnable() { }
}

internal class SpringBaker : Baker<SpringComponent>
{
    public override void Bake(SpringComponent authoring)
    {
        if (authoring.enabled)
        {
            var physicsData = new Spring
            {
                nodeEntity = GetEntity(),
                nodeParentEntity = GetEntity(authoring.transform.parent),
                nodeParentOffset = authoring.transform.localPosition,
                strength = authoring.strength,
                damping = authoring.damping
            };

            var physicsEntity = CreateAdditionalEntity();
            AddComponent(physicsEntity, physicsData);
        }
    }
}
