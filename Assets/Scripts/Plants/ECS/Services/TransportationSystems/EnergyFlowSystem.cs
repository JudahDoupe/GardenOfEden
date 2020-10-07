using Assets.Scripts.Plants.ECS.Components;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.Plants.ECS.Services.TransportationSystems
{
    public struct EnergyStore : IComponentData
    {
        public float Quantity { get; set; }
        public float Capacity { get; set; }
    }

    public struct EnergyFlow : IComponentData
    {
        public float FlowRate { get; set; }
        public float Throughput { get; set; }
    }

    [InternalBufferCapacity(5)]
    public struct BranchBufferElement : IBufferElementData
    {
        public static implicit operator Entity(BranchBufferElement e) { return e.Value; }
        public static implicit operator BranchBufferElement(Entity e) { return new BranchBufferElement { Value = e }; }

        public Entity Value;
    }

    class EnergyFlowSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var parallelWriter = ecb.AsParallelWriter();

            var jobHandle = Entities
                .WithChangeFilter<BranchBufferElement>()
                .ForEach(
                    (in DynamicBuffer<Child> branches, in int entityInQueryIndex) =>
                    {
                        var internodeQuery = GetComponentDataFromEntity<InternodeReference>(true);

                        for (int i = 0; i < branches.Length; i++)
                        {
                            var branchEntity = branches[i].Value;
                            var internode = internodeQuery[branchEntity].Internode;
                            var energyFlow = new EnergyFlow { FlowRate = 1f / (branches.Length + 1), Throughput = 0 };
                            parallelWriter.SetComponent(entityInQueryIndex, internode, energyFlow);
                        }
                    })
                .WithBurst()
                .WithName("UpdateEnergyFlowRate")
                .ScheduleParallel(Dependency);

            jobHandle.Complete();
            ecb.Playback(EntityManager);
            ecb.Dispose();

            Entities
                .ForEach(
                    (ref EnergyFlow energyFlow, in Internode internode) =>
                    {
                        var query = GetComponentDataFromEntity<EnergyStore>(true);

                        var headStore = query[internode.HeadNode];
                        var tailStore = query[internode.TailNode];

                        var headPressure = headStore.Quantity / headStore.Capacity;
                        var tailPressure = tailStore.Quantity / tailStore.Capacity;

                        if (tailPressure > headPressure)
                        {
                            energyFlow.Throughput = energyFlow.FlowRate * tailStore.Quantity * (tailPressure - headPressure);
                        }
                        else
                        {
                            energyFlow.Throughput = -energyFlow.FlowRate * headStore.Quantity * (headPressure - tailPressure);
                        }
                    })
                .WithName("CalculateEnergyThroughput")
                .ScheduleParallel();

            Entities
                .ForEach(
                    (ref EnergyStore energyStore, in InternodeReference internodeRef, in DynamicBuffer<BranchBufferElement> branches) =>
                    {
                        var internodeQuery = GetComponentDataFromEntity<InternodeReference>(true);
                        var energyFlowQuery = GetComponentDataFromEntity<EnergyFlow>(true);

                        if (energyFlowQuery.HasComponent(internodeRef.Internode))
                        {
                            energyStore.Quantity += energyFlowQuery[internodeRef.Internode].Throughput;
                        }

                        for (int i = 0; i < branches.Length; i++)
                        {
                            var internode = internodeQuery[branches[i].Value].Internode;
                            energyStore.Quantity -= energyFlowQuery[internode].Throughput;
                        }
                    })
                .WithName("UpdateEnergyQuantities")
                .ScheduleParallel();
        }

    }
}
