using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct PhysicsAspect : IAspect
{
    public readonly TransformAspect Transform;
    public readonly RefRW<SphereCollider> Collider;
    public readonly RefRW<SpringJoint> SpringJoint;
    public readonly RefRW<Force> Force;

    public void AddForce(float3 force)
    {
        Force.ValueRW.Value += force;
    }

    public float3 CurrentLocalPosition
    {
        get => Transform.LocalPosition;
        set => Transform.LocalPosition = value;
    }

    public float3 ProjectedLocalPosition => CurrentLocalPosition + Force.ValueRO.Value;
    public float3 ProjectedWorldPosition => Transform.TransformPointLocalToWorld(ProjectedLocalPosition);
    public float MinDistance => Collider.ValueRO.Radius;
    public float ConstrainedLength => math.length(EquilibriumPosition);
    public float Stiffness => SpringJoint.ValueRO.Stiffness;

    public float3 EquilibriumPosition
    {
        get => SpringJoint.ValueRO.EquilibriumPosition;
        set => SpringJoint.ValueRW.EquilibriumPosition = value;
    }
}