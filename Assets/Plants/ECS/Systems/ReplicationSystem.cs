using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ReplicationSystem : ISystem
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
        using var ecb = new EntityCommandBuffer(Allocator.TempJob);
        new InstantiateStructureJob
        {
            Ecb = ecb,
        }.Run();
        ecb.Playback(state.EntityManager);
    }
}

[BurstCompile]
public partial struct InstantiateStructureJob : IJobEntity
{
    public EntityCommandBuffer Ecb;

    [BurstCompile]
    private void Execute(Entity entity, ReplicationAspect division)
    {
        if (!division.IsReadyToDivide) return;
        
        var newNode = Ecb.Instantiate(division.SupportStructure);
        Ecb.AddComponent(newNode, new Parent()
        {
            Value = division.Parent,
        });
        Ecb.SetComponent(newNode, division.Transform.LocalTransform);
        Ecb.SetComponent(newNode, division.Dna);
        Ecb.DestroyEntity(entity);
        
        //We remove the linked entity groups so that they can be reinitialized
        Ecb.RemoveComponent<LinkedEntityGroup>(newNode);
        Ecb.RemoveComponent<LinkedEntityGroup>(division.Parent);
    }
}
