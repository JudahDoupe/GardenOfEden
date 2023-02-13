using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DnaSystem : ISystem
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
        var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (copyDna, e) in SystemAPI.Query<RefRO<CopyDna>>()
                                              .WithAll<Dna>()
                                              .WithEntityAccess())
        {
            var originalDna = SystemAPI.GetComponent<Dna>(copyDna.ValueRO.DnaSource);
            ecb.SetComponent(e, originalDna);
            ecb.RemoveComponent<CopyDna>(e);
        }
    }
}