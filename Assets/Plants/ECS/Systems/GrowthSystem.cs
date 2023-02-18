using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(PlantSimulationGroup))]
[UpdateAfter(typeof(ReplicationSystem))]
public partial struct GrowthSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        new PrimaryGrowthJob
        {
            DeltaTime = deltaTime
        }.Run();
    }
}

[BurstCompile]
public partial struct PrimaryGrowthJob : IJobEntity
{
    public float DeltaTime;

    [BurstCompile]
    private void Execute(GrowthAspect growth)
    {
        growth.Grow(DeltaTime);
    }
}