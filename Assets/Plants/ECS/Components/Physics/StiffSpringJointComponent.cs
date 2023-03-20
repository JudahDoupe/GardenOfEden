using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct StiffSpringJoint : IComponentData
{
    public float Stiffness;
    public float3 EquilibriumPosition;
}


public class StiffSpringJointComponent : MonoBehaviour
{
    public float Stiffness;
}

public class StiffSpringJointComponentBaker : Baker<StiffSpringJointComponent>
{
    public override void Bake(StiffSpringJointComponent authoring)
    {
        AddComponent(new StiffSpringJoint
        {
            Stiffness = authoring.Stiffness,
            EquilibriumPosition = authoring.transform.localPosition,
        });
    }
}