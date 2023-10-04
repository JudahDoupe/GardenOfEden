using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public class FixedLengthConstraintComponent : MonoBehaviour { }

public class FixedLengthConstraintComponentBaker : Baker<FixedLengthConstraintComponent>
{
    public override void Bake(FixedLengthConstraintComponent authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(e, new LengthConstraint
        {
            Length = authoring.transform.localPosition.magnitude,
        });
        AddComponent(e, new ConstraintResponse
        {
            PositionAdjustment = new float3(0,0,0),
        });
    }
}