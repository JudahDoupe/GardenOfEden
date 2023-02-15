using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PrimaryGrowthSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        new PrimaryGrowthJob
        {
            DeltaTime = deltaTime,
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
        growth.Size.ValueRW.NodeRadius = math.lerp(growth.Size.ValueRO.NodeRadius, growth.PrimaryGrowthTarget.ValueRO.NodeRadius, DeltaTime);
        growth.Size.ValueRW.InternodeLength = math.lerp(growth.Size.ValueRO.InternodeLength, growth.PrimaryGrowthTarget.ValueRO.InternodeLength, DeltaTime);
        growth.Transform.LocalPosition = new float3(0,0, growth.Size.ValueRO.InternodeLength);
    }
}