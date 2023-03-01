using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(PlantSimulationGroup))]
[UpdateAfter(typeof(ReplicationSystem))]
public partial struct CopyDnaSystem : ISystem
{
    private ComponentLookup<Dna> _dnaLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _dnaLookup = SystemAPI.GetComponentLookup<Dna>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                                     .CreateCommandBuffer(state.WorldUnmanaged);
        _dnaLookup.Update(ref state);

        foreach (var (dnaSource, dna, entity) in SystemAPI.Query<RefRO<DnaSource>, RefRW<Dna>>().WithEntityAccess())
        {
            var sourceDna = _dnaLookup[dnaSource.ValueRO.Source];

            dna.ValueRW.SupportStructurePrefab = sourceDna.SupportStructurePrefab;

            commandBuffer.RemoveComponent<DnaSource>(entity);
        }
    }
}