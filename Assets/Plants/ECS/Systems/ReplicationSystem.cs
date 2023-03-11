using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PlantSimulationGroup))]
public partial struct ReplicationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        using var ecb = new EntityCommandBuffer(Allocator.TempJob);
        state.Dependency.Complete(); 
        new InstantiateStructureJob
        {
            Ecb = ecb,
        }.Run();
        ecb.Playback(state.EntityManager);
    }
}

public partial struct InstantiateStructureJob : IJobEntity
{
    public EntityCommandBuffer Ecb;

    [BurstCompile]
    private void Execute(Entity entity, ReplicationAspect division)
    {
        if (!division.IsReadyToDivide) return;

        var newNode = Ecb.Instantiate(division.SupportStructure);
        Ecb.SetComponent(newNode, division.Transform.WorldTransform);
        Ecb.SetComponent(newNode, division.Dna);
        Ecb.SetComponent(newNode, new BaseNode{ Entity = division.ConnectionPoint });

        //TODO: we probably want to make the entity to kill itself over time
        //Ecb.DestroyEntity(entity);
        Ecb.RemoveComponent<Replicator>(entity);

        //TODO: fix the linked entity buffer
    }
}