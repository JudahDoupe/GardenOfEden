using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct RelinkNodesSystem : ISystem
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
        var endInitialization = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var unlinkedNodes = SystemAPI.QueryBuilder()
                                     .WithNone<LinkedEntityGroup>()
                                     .WithAll<Child>()
                                     .Build();
        var cmd = new EntityCommandBuffer(Allocator.Temp);
        new RelinkNewNodesJob
        {
            Ecb = endInitialization.CreateCommandBuffer(state.WorldUnmanaged),
            ChildLookup = state.GetBufferLookup<Child>(isReadOnly: true)
        }.Run(unlinkedNodes);
    }
}

[BurstCompile]
public partial struct RelinkNewNodesJob : IJobEntity
{
    public EntityCommandBuffer Ecb;

    [ReadOnly] public BufferLookup<Child> ChildLookup;


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