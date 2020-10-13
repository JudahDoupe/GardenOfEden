using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.ECS.Services.TransportationSystems
{
    public struct EnergyStore : IComponentData
    {
        public float Quantity;
        public float Capacity;
    }

    public struct EnergyFlow : IComponentData
    {
        public float Throughput;
    }

    public class EnergyFlowSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                    (ref EnergyFlow energyFlow, in Internode internode) =>
                    {
                        var energyStoreQuery = GetComponentDataFromEntity<EnergyStore>(true);
                        var childrenQuery = GetBufferFromEntity<Child>(true);
                        var internodeEntityQuery = GetComponentDataFromEntity<InternodeReference>(true);
                        var internodeQuery = GetComponentDataFromEntity<Internode>(true);

                        var headInternode = internodeQuery[internodeEntityQuery[internode.HeadNode].Internode];
                        var tailInternode = internodeQuery[internodeEntityQuery[internode.HeadNode].Internode];

                        var headStore = energyStoreQuery[internode.HeadNode];
                        var tailStore = energyStoreQuery[internode.TailNode];
                        var branches = childrenQuery[internode.TailNode];

                        var resistence = 1f;
                        var flowrate = (1f / (branches.Length + 1)) / resistence;
                        var headPressure = headStore.Quantity / (headStore.Capacity + GetInternodeCapacity(headInternode) + float.Epsilon);
                        var tailPressure = tailStore.Quantity / (tailStore.Capacity + GetInternodeCapacity(tailInternode) + float.Epsilon);

                        if (tailPressure > headPressure)
                        {
                            energyFlow.Throughput = flowrate * tailStore.Quantity * (tailPressure - headPressure);
                        }
                        else
                        {
                            energyFlow.Throughput = -flowrate * headStore.Quantity * (headPressure - tailPressure);
                        }
                    })
                .WithName("UpdateEnergyThroughput")
                .ScheduleParallel();

            Entities
                .ForEach(
                    (ref EnergyStore energyStore, in InternodeReference internodeRef, in DynamicBuffer<Child> branches) =>
                    {
                        var internodeRefQuery = GetComponentDataFromEntity<InternodeReference>(true);
                        var internodeQuery = GetComponentDataFromEntity<Internode>(true);
                        var energyFlowQuery = GetComponentDataFromEntity<EnergyFlow>(true);

                        if (energyFlowQuery.HasComponent(internodeRef.Internode))
                        {
                            energyStore.Quantity += energyFlowQuery[internodeRef.Internode].Throughput;
                        }

                        for (int i = 0; i < branches.Length; i++)
                        {
                            var internodeEntity = internodeRefQuery[branches[i].Value].Internode;
                            energyStore.Quantity -= energyFlowQuery[internodeEntity].Throughput;
                        }

                        var maxCapacity = energyStore.Capacity + GetInternodeCapacity(internodeQuery[internodeRef.Internode]);
                        energyStore.Quantity = math.clamp(energyStore.Quantity, 0, maxCapacity);
                    })
                .WithName("UpdateEnergyQuantities")
                .ScheduleParallel();
        }


        private static float GetInternodeCapacity(Internode internode)
        {
            return internode.Length* internode.Radius* internode.Radius* math.PI * 0.3f;
        }
    }
}
