using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public class ChainJointComponent : MonoBehaviour { }

public class ChainJointComponentBaker : Baker<ChainJointComponent>
{
    public override void Bake(ChainJointComponent authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);
        var back = quaternion.LookRotationSafe(-authoring.transform.localPosition, authoring.transform.position);

        AddComponent(e, new LengthConstraint
        {
            Length = authoring.transform.localPosition.magnitude,
        });
        
        AddComponent(e, new FaceAwayFromParentConstraint()
        {
            InitialRotation = Quaternion.Inverse(back) * authoring.transform.localRotation,
        });

        AddComponent(e, new ConstraintResponse
        {
            PositionAdjustment = new float3(0,0,0),
            VelocityAdjustment = new float3(0,0,0),
        });
    }
}