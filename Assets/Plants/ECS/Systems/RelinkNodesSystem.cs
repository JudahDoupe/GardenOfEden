using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PlantSimulationGroup))]
[UpdateAfter(typeof(ReplicationSystem))]
public partial struct RelinkNodesSystem : ISystem
{
    private BufferLookup<Child> _childLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _childLookup = state.GetBufferLookup<Child>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var endSimulation = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var unlinkedNodes = SystemAPI.QueryBuilder()
                                     .WithNone<LinkedEntityGroup>()
                                     .WithAll<Child>()
                                     .Build();
        _childLookup.Update(ref state);
        state.Dependency = new RelinkNewNodesJob
        {
            Ecb = endSimulation.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            ChildLookup = _childLookup
        }.ScheduleParallel(unlinkedNodes, state.Dependency);
    }
}

public partial struct RelinkNewNodesJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    [ReadOnly]
    public BufferLookup<Child> ChildLookup;


    [BurstCompile]
    private void Execute(Entity entity)
    {
        Ecb.AddBuffer<LinkedEntityGroup>(entity.Index, entity);

        if (!ChildLookup.TryGetBuffer(entity, out var children))
            return;

        Ecb.AppendToBuffer(entity.Index, entity, new LinkedEntityGroup { Value = entity });

        for (int i = 0, childCount = children.Length; i < childCount; i++)
        {
            Ecb.AppendToBuffer(entity.Index, entity, new LinkedEntityGroup { Value = children[i].Value });
        }
    }
}