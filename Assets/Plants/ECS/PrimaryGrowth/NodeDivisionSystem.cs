using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct NodeDivisionSystem : ISystem
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
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        
        new NodeDivisionJob
        {
            Ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
        }.Run();
    }
}

[BurstCompile]
public partial struct NodeDivisionJob : IJobEntity
{
    public EntityCommandBuffer Ecb;

    [BurstCompile]
    private void Execute(NodeDivisionAspect division)
    {
        if (!division.IsReadyToDivide) return;
        
        var newNode = Ecb.Instantiate(division.Structure);
        Ecb.AddComponent(newNode, new Parent()
        {
            Value = division.Parent,
        });
        Ecb.SetComponent(newNode, division.Transform.LocalTransform);
        Ecb.DestroyEntity(division.Entity);
    }
}