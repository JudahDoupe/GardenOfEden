using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct StiffSpringJoint : IComponentData
{
    public float Stiffness;
    public float Dampening;
    public float3 EquilibriumPosition;
    public quaternion TargetRotation;
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
        AddComponent(new StiffSpringJoint
        {
            Stiffness = authoring.Stiffness,
            Dampening = authoring.Dampening,
            EquilibriumPosition = authoring.transform.localPosition,
            TargetRotation = Quaternion.Inverse(back) * authoring.transform.localRotation,
        });
    }
}