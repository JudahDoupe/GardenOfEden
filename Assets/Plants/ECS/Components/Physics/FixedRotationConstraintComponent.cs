using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public class FixedRotationConstraintComponent : MonoBehaviour { }

public class FixedRotationConstraintComponentBaker : Baker<FixedRotationConstraintComponent>
{
    public override void Bake(FixedRotationConstraintComponent authoring)
    {       
        var e = GetEntity(TransformUsageFlags.Dynamic);
        var back = quaternion.LookRotationSafe(-authoring.transform.localPosition, authoring.transform.position);

        AddComponent(e, new FaceAwayFromParentConstraint()
        {
            InitialRotation = Quaternion.Inverse(back) * authoring.transform.localRotation,
        });
    }
}