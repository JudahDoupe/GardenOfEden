using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct PhysicsAspect : IAspect
{
    public readonly TransformAspect Transform;
    private float3 ProjectedLocalPosition(float deltaTime) => Transform.LocalPosition + _physics.ValueRO.Velocity * deltaTime;

    private readonly RefRW<SphereCollider> _collider;
    private readonly RefRW<Physics> _physics;

    [Optional]
    private readonly RefRW<SpringJoint> _springJoint;

    public void AddForce(float3 vector, float deltaTime)
    {
        _physics.ValueRW.Force += vector;
        _physics.ValueRW.Velocity = _physics.ValueRO.Force / _physics.ValueRO.Mass * deltaTime;
    }

    public void Move(float3 vector, float deltaTime)
    {
        var targetPosition = ProjectedLocalPosition(deltaTime) + vector;
        MoveTo(targetPosition, deltaTime);
    }

    public void MoveTo(float3 localPosition, float deltaTime)
    {
        _physics.ValueRW.Velocity = (localPosition - Transform.LocalPosition) / deltaTime;
        _physics.ValueRW.Force = _physics.ValueRO.Mass * (_physics.ValueRW.Velocity / deltaTime);
    }

    public void UpdatePosition(float deltaTime)
    {
        AddGravityForce(deltaTime);
        AddSpringForce(deltaTime);
        CollideWithGround(deltaTime);
        ConstrainSpringLength(deltaTime);

        Transform.LocalPosition += _physics.ValueRO.Velocity * deltaTime;
        if (!Transform.LocalPosition.Equals(float3.zero) && _springJoint.IsValid)
            Transform.LocalRotation = quaternion.LookRotationSafe(Transform.LocalPosition, math.up());
    }


    private void AddGravityForce(float deltaTime)
    {
        var direction = Transform.TransformDirectionWorldToParent(math.down());
        var magnitude = _physics.ValueRO.Mass * 9.8f;
        AddForce(direction * magnitude, deltaTime);
    }

    private void AddSpringForce(float deltaTime)
    {
        if (!_springJoint.IsValid) return;

        var vector = (_springJoint.ValueRO.EquilibriumPosition - ProjectedLocalPosition(deltaTime)) * _springJoint.ValueRO.Stiffness;
        Move(vector, deltaTime);
    }

    private void ConstrainSpringLength(float deltaTime)
    {
        if (!_springJoint.IsValid) return;

        var targetPosition = ProjectedLocalPosition(deltaTime);
        if (!targetPosition.Equals(float3.zero))
            targetPosition = math.normalize(targetPosition) * _springJoint.ValueRO.FixedLength;

        MoveTo(targetPosition, deltaTime);
    }

    private void CollideWithGround(float deltaTime)
    {
        var direction = Transform.TransformDirectionWorldToParent(math.up());
        var magnitude = math.max(0, _collider.ValueRO.Radius - Transform.TransformPointLocalToWorld(ProjectedLocalPosition(deltaTime)).y);
        Move(direction * magnitude, deltaTime);
    }

    public void UpdateSpring(float3 equilibriumPosition, float length)
    {
        if (!_springJoint.IsValid) return;

        _springJoint.ValueRW.EquilibriumPosition = equilibriumPosition;
        _springJoint.ValueRW.FixedLength = length;
    }

    public void UpdateCollider(float radius)
    {
        _collider.ValueRW.Radius = radius;
    }
}