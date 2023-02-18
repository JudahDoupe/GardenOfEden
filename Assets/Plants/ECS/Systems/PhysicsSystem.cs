using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct PhysicsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        new ApplyGravity
        {
            DeltaTime = deltaTime,
            Gravity = 9.8f
        }.Run();
        new ApplyPlanetResistance().Run();
        new ApplySpringForce().Run();
        new IntegrateForce
        {
            DeltaTime = deltaTime
        }.Run();
    }
}

[BurstCompile]
public partial struct ApplyGravity : IJobEntity
{
    public float DeltaTime;
    public float Gravity;

    [BurstCompile]
    private void Execute(PhysicsAspect physics)
    {
        physics.AddForce(math.down() * DeltaTime * Gravity);
    }
}

[BurstCompile]
public partial struct ApplyPlanetResistance : IJobEntity
{
    [BurstCompile]
    private void Execute(PhysicsAspect physics)
    {
        var earthOverlap = math.max(physics.MinDistance - physics.ProjectedWorldPosition.y, 0);
        physics.AddForce(math.up() * earthOverlap);
    }
}

[BurstCompile]
public partial struct ApplySpringForce : IJobEntity
{
    [BurstCompile]
    private void Execute(PhysicsAspect physics)
    {
        physics.AddForce(-physics.Stiffness * physics.Force.ValueRO.Value);
    }
}

[BurstCompile]
public partial struct IntegrateForce : IJobEntity
{
    public float DeltaTime;

    [BurstCompile]
    private void Execute(PhysicsAspect physics)
    {
        var length = physics.ConstrainedLength;
        if (length < 0.01f)
        {
            physics.Force.ValueRW.Value = 0;
            return;
        }

        var projectedPosition = physics.ProjectedLocalPosition;
        var targetPosition = math.normalize(projectedPosition) * physics.ConstrainedLength;
        physics.AddForce(targetPosition - projectedPosition);
        physics.CurrentLocalPosition = physics.ProjectedLocalPosition;
        
        if (length > 0.01f) physics.Transform.LocalRotation = quaternion.LookRotationSafe(physics.CurrentLocalPosition, math.up());
        physics.CurrentLocalPosition = physics.Transform.LocalTransform.Forward() * length;
    }
}