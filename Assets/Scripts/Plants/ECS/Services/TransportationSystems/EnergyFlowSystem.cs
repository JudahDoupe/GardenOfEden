using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;

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
        private EndSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithChangeFilter<BranchBufferElement>()
                .ForEach(
                    (in DynamicBuffer<BranchBufferElement> branches, in int entityInQueryIndex) =>
                    {
                        var internodeQuery = GetComponentDataFromEntity<InternodeReference>(true);

                        for (int i = 0; i < branches.Length; i++)
                        {
                            var branchEntity = branches[i].Value;
                            var internode = internodeQuery[branchEntity].Internode;
                            var energyFlow = new EnergyFlow { FlowRate = 1f / (branches.Length + 1), Throughput = 0 };
                            ecb.SetComponent(entityInQueryIndex, internode, energyFlow);
                        }
                    })
                .WithBurst()
                .WithName("UpdateEnergyFlowrate")
                .ScheduleParallel();

            //TODO: Replay ECB before continuing energy flow

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
                .WithBurst()
                .WithName("CalculateEnergyThroughput")
                .ScheduleParallel();

            Entities
                .ForEach(
                    (ref EnergyStore energyStore, in InternodeReference internodeRef, in DynamicBuffer<BranchBufferElement> branches) =>
                    {
                        var internodeQuery = GetComponentDataFromEntity<InternodeReference>(true);
                        var energyFlowQuery = GetComponentDataFromEntity<EnergyFlow>(true);

                        energyStore.Quantity += energyFlowQuery[internodeRef.Internode].Throughput;

                        for (int i = 0; i < branches.Length; i++)
                        {
                            var internode = internodeQuery[branches[i].Value].Internode;
                            energyStore.Quantity -= energyFlowQuery[internode].Throughput;
                        }
                    })
                .WithBurst()
                .WithName("UpdateEnergyQuantities")
                .ScheduleParallel();
        }

    }
}
