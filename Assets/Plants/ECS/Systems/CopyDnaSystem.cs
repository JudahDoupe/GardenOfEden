using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CopyDnaSystem : ISystem
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
        var beginSimulation = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                                       .CreateCommandBuffer(state.WorldUnmanaged);
        var dnaLookup = SystemAPI.GetComponentLookup<Dna>(isReadOnly: true);

        foreach (var (dnaSource, dna, entity) in SystemAPI.Query<RefRO<DnaSource>, RefRW<Dna>>().WithEntityAccess())
        {
            var sourceDna = dnaLookup[dnaSource.ValueRO.Source];
            dna.ValueRW.SupportStructurePrefab = sourceDna.SupportStructurePrefab;
            beginSimulation.RemoveComponent<DnaSource>(entity);
        }
    }
}