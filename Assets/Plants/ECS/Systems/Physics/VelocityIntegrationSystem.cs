using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

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

        state.Dependency = new IntegrateVelocityEuler
        {
            TimeStep = deltaTime
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct IntegrateVelocityEuler : IJobEntity
{
    public float TimeStep;

    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics,
                         TransformAspect transform)
    {
        transform.LocalPosition += physics.ValueRO.Velocity * TimeStep;

        physics.ValueRW.Velocity += physics.ValueRO.Force / physics.ValueRO.Mass * TimeStep;
        physics.ValueRW.Force = 0;
    }
}