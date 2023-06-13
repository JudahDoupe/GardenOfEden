using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

[BurstCompile]
[UpdateInGroup(typeof(PlantSimulationGroup))]
public partial struct ReplicationSystem : ISystem
{
    private BufferLookup<Child> _childLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _childLookup = state.GetBufferLookup<Child>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        return;
        var endSimulation = SystemAPI.GetSingleton<EndVariableRateSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        _childLookup.Update(ref state);

        state.Dependency = new InstantiateStructureJob
        {
            Ecb = endSimulation.AsParallelWriter(),
            ChildLookup = _childLookup
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct InstantiateStructureJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    [ReadOnly]
    public BufferLookup<Child> ChildLookup;

    [BurstCompile]
    private void Execute(Entity entity,
                         ReplicationAspect division)
    {
        if (!division.IsReadyToDivide) return;

        var newNode = Ecb.Instantiate(entity.Index, division.SupportStructure);
        Ecb.AddComponent(entity.Index, newNode, new Parent { Value = division.Parent });
        Ecb.SetComponent(entity.Index, newNode, division.LocalTransform);
        Ecb.SetComponent(entity.Index, newNode, division.Dna);

        DestroyEntitiesRecursively(entity.Index, entity);
    }

    private void DestroyEntitiesRecursively(int index, Entity e)
    {
        if (ChildLookup.TryGetBuffer(e, out var children))
            for (var i = 0; i < children.Length; i++)
                DestroyEntitiesRecursively(index, children[i].Value);

        Ecb.DestroyEntity(index, e);
    }
}