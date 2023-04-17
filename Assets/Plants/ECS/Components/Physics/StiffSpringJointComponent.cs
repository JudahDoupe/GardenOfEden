using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SpringJoint : IComponentData
{
    public float Stiffness;
    public float Dampening;
    public float3 EquilibriumPosition;
    public quaternion TargetRotation;
}

public struct LengthConstraint : IComponentData
{
    public float Length;
}

public struct ConstraintResponse : IComponentData
{
    public float3 PositionAdjustment;
    public float3 VelocityAdjustment;
}

public class StiffSpringJointComponent : MonoBehaviour
{
    public float Stiffness;
    public float Dampening;
}

public class StiffSpringJointComponentBaker : Baker<StiffSpringJointComponent>
{
    public override void Bake(StiffSpringJointComponent authoring)
    {
        var back = quaternion.LookRotationSafe(-authoring.transform.localPosition, authoring.transform.position);

        AddComponent(new SpringJoint
        {
            Stiffness = authoring.Stiffness,
            Dampening = authoring.Dampening,
            EquilibriumPosition = authoring.transform.localPosition,
            TargetRotation = Quaternion.Inverse(back) * authoring.transform.localRotation,
        });
        
        AddComponent(new LengthConstraint
        {
            Length = authoring.transform.localPosition.magnitude,
        });
        
        AddComponent(new ConstraintResponse
        {
            PositionAdjustment = new float3(0,0,0),
            VelocityAdjustment = new float3(0,0,0),
        });
    }
}