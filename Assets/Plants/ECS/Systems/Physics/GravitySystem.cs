using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

// ReSharper disable PartialTypeWithSinglePart

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(VelocityIntegrationSystem))]
public partial struct GravitySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new AddGravity()
            .Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct AddGravity : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics)
    {
        physics.ValueRW.AddAcceleration(new float3(0, -9.8f, 0));
    }
}