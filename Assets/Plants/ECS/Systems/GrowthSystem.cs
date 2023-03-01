using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(PlantSimulationGroup))]
[UpdateAfter(typeof(ReplicationSystem))]
public partial struct GrowthSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        state.Dependency = new PrimaryGrowthJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(state.Dependency);
    }
}

public partial struct PrimaryGrowthJob : IJobEntity
{
    public float DeltaTime;

    [BurstCompile]
    private void Execute(GrowthAspect growth)
    {
        growth.Grow(DeltaTime);
    }
}