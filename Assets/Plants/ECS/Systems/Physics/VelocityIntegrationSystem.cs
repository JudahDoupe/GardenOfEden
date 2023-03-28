using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct VelocityIntegrationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.fixedDeltaTime;

        state.Dependency = new AddGravity()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new AddSpringForce()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new IntegrateVelocityEuler
        {
            TimeStep = deltaTime
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct AddGravity : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics)
    {
        physics.ValueRW.Force += new float3(0, -9.8f, 0) * physics.ValueRO.Mass;
    }
}

[BurstCompile]
public partial struct AddSpringForce : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics) { }
}

[BurstCompile]
public partial struct IntegrateVelocityEuler : IJobEntity
{
    public float TimeStep;

    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics,
                         TransformAspect transform)
    {
        transform.WorldPosition += physics.ValueRO.Velocity * TimeStep;

        physics.ValueRW.Velocity += physics.ValueRO.Force / physics.ValueRO.Mass * TimeStep;
        physics.ValueRW.Force = 0;
    }
}