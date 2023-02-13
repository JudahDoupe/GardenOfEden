using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DnaSystem : ISystem
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
    }
}

[BurstCompile]
public partial struct DnaJob : IJobEntity
{
    [BurstCompile]
    private void Execute(Parent parent, PrimaryGrowthTarget growth)
    {
        //TODO: if a parent has DNA but the entity does not, copy dna
    }
}