using Unity.Burst;
using Unity.Entities;

[UpdateBefore(typeof(VelocityIntegrationSystem))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct ConstraintSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.fixedDeltaTime;

        state.Dependency = new SolveLengthConstraint()
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct SolveLengthConstraint : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics) { }
}