using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct NodePhysicsAspect : IAspect
{
    private readonly RefRW<PhysicsBody> _body;
    private readonly RefRW<SpringJoint> _spring;
    private readonly RefRW<CapsuleCollider> _collider;
    private readonly RefRW<LengthConstraint> _lengthConstraint;

    public void UpdateSize(float length,
                           float radius,
                           float mass)
    {
        _body.ValueRW.Mass = mass;
        
        _collider.ValueRW.Length = length;
        _collider.ValueRW.Radius = radius;
        _collider.ValueRW.End = float3.zero;
        
        _lengthConstraint.ValueRW.Length = length;
        
        if (_spring.ValueRO.EquilibriumPosition.Equals(float3.zero))
        {
            _collider.ValueRW.Start = float3.zero;
        }
        else
        {
            var forward = math.normalize(_spring.ValueRO.EquilibriumPosition);
            _spring.ValueRW.EquilibriumPosition = forward * length;
            _collider.ValueRW.Start = -forward * length;
        }

        _spring.ValueRW.Stiffness = mass;
    }
}