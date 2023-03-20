using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct PhysicsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.fixedDeltaTime;

        state.Dependency = new GravityJob().ScheduleParallel(state.Dependency);
        state.Dependency = new IntegrateForcesEuler
        {
            TimeStep = deltaTime,
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct GravityJob : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<Physics> physics)
    {
        physics.ValueRW.Force += new float3(0, -9.8f, 0) * physics.ValueRO.Mass;
    }
}

[BurstCompile]
public partial struct IntegrateForcesEuler : IJobEntity
{
    public float TimeStep;
    
    [BurstCompile]
    private void Execute(RefRW<Physics> physics, TransformAspect transform)
    {
        physics.ValueRW.Velocity += physics.ValueRO.Force / physics.ValueRO.Mass * TimeStep;
        transform.WorldPosition += physics.ValueRO.Velocity * TimeStep;

        physics.ValueRW.Force = 0;
    }
}