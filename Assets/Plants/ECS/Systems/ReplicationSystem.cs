using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PlantSimulationGroup))]
public partial struct ReplicationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        using var ecb = new EntityCommandBuffer(Allocator.TempJob);
        state.Dependency = new InstantiateStructureJob
        {
            Ecb = ecb.AsParallelWriter()
        }.ScheduleParallel(state.Dependency);
        ecb.Playback(state.EntityManager);
    }
}

[BurstCompile]
public partial struct InstantiateStructureJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;

    [BurstCompile]
    private void Execute(Entity entity, ReplicationAspect division)
    {
        if (!division.IsReadyToDivide) return;

        var newNode = Ecb.Instantiate(entity.Index, division.SupportStructure);
        Ecb.AddComponent(entity.Index, newNode, new Parent
        {
            Value = division.Parent
        });
        Ecb.SetComponent(entity.Index, newNode, division.LocalTransform);
        Ecb.SetComponent(entity.Index, newNode, division.Dna);

        //TODO: we probably want to make the entity to kill itself over time
        //Ecb.DestroyEntity(entity);
        Ecb.RemoveComponent<Replicator>(entity.Index, entity);

        //We remove the linked entity groups so that they can be reinitialized
        Ecb.RemoveComponent<LinkedEntityGroup>(entity.Index, newNode);
        Ecb.RemoveComponent<LinkedEntityGroup>(entity.Index, division.Parent);
    }
}