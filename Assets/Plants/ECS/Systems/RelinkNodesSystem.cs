using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct RelinkNodesSystem : ISystem
{
    private BufferLookup<Child> _childLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _childLookup = state.GetBufferLookup<Child>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var endInitialization = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var unlinkedNodes = SystemAPI.QueryBuilder()
                                     .WithNone<LinkedEntityGroup>()
                                     .WithAll<Child>()
                                     .Build();
        _childLookup.Update(ref state);
        new RelinkNewNodesJob
        {
            Ecb = endInitialization.CreateCommandBuffer(state.WorldUnmanaged),
            ChildLookup = _childLookup
        }.Run(unlinkedNodes);
    }
}

[BurstCompile]
public partial struct RelinkNewNodesJob : IJobEntity
{
    public EntityCommandBuffer Ecb;

    [ReadOnly]
    public BufferLookup<Child> ChildLookup;


    [BurstCompile]
    private void Execute(Entity entity)
    {
        Ecb.AddBuffer<LinkedEntityGroup>(entity);

        if (!ChildLookup.TryGetBuffer(entity, out var children))
            return;

        Ecb.AppendToBuffer(entity, new LinkedEntityGroup { Value = entity });

        for (int i = 0, childCount = children.Length; i < childCount; i++)
        {
            Ecb.AppendToBuffer(entity, new LinkedEntityGroup { Value = children[i].Value });
        }
    }
}