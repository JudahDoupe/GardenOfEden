using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(PlantSimulationGroup))]
[UpdateAfter(typeof(ReplicationSystem))]
public partial struct InitializeNodePhysicsSystem : ISystem
{
    private ComponentLookup<BaseNode> _baseNodeLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _baseNodeLookup = state.GetComponentLookup<BaseNode>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _baseNodeLookup.Update(ref state);
        var endSimulation = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var uninitializedNodesQuery = SystemAPI.QueryBuilder()
                                     .WithAll<InitializePhysics>()
                                     .Build();
        
        state.Dependency = new InitializeNodePhysicsSystemJob
        {
            Ecb = endSimulation.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            BaseNodeLookup = _baseNodeLookup,
        }.ScheduleParallel(uninitializedNodesQuery, state.Dependency);
    }
}

public partial struct InitializeNodePhysicsSystemJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    [ReadOnly]
    public ComponentLookup<BaseNode> BaseNodeLookup;

    [BurstCompile]
    private void Execute(Entity entity, NodePhysicsAspect physics)
    {
        physics.BaseNode = BaseNodeLookup[physics.Node].Entity;
        Ecb.RemoveComponent<InitializePhysics>(entity.Index, entity);
    }
}