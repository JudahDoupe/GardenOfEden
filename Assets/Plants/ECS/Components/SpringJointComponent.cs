using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SpringJoint : IComponentData
{
    public float Stiffness;
    public float3 EquilibriumPosition;
    public float FixedLength;
}


public class SpringJointComponent : MonoBehaviour
{
    public float Stiffness;
}

public class SpringJointComponentBaker : Baker<SpringJointComponent>
{
    public override void Bake(SpringJointComponent authoring)
    {
        AddComponent(new SpringJoint
        {
            Stiffness = authoring.Stiffness,
            EquilibriumPosition = float3.zero,
            FixedLength = 0,
        });
    }
}