using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    public struct EnergyStore : IComponentData
    {
        public float Quantity;
        public float Capacity;
        public float Pressure => Quantity / (Capacity + float.Epsilon);
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
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach(
                    (ref EnergyFlow energyFlow, in EnergyStore energyStore, in Entity entity) =>
                    {
                        var energyStoreQuery = GetComponentDataFromEntity<EnergyStore>(true);
                        var parentQuery = GetComponentDataFromEntity<Parent>(true);
                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        if (!parentQuery.HasComponent(entity)
                            || parentQuery[entity].Value == Entity.Null
                            || !energyStoreQuery.HasComponent(parentQuery[entity].Value)
                            || !childrenQuery.HasComponent(parentQuery[entity].Value))
                        {
                            energyFlow.Throughput = 0;
                        }
                        else
                        {
                            var headStore = energyStore;
                            var tailStore = energyStoreQuery[parentQuery[entity].Value];
                            var numBranches = childrenQuery[parentQuery[entity].Value].Length + 1;

                            var resistance = 0f;
                            var flowRate = (1f / numBranches) / (1 + resistance);

                            if (tailStore.Pressure > headStore.Pressure)
                            {
                                energyFlow.Throughput = flowRate * tailStore.Quantity *
                                                        (tailStore.Pressure - headStore.Pressure);
                            }
                            else
                            {
                                energyFlow.Throughput = flowRate * headStore.Quantity *
                                                        (tailStore.Pressure - headStore.Pressure);
                            }
                        }
                    })
                .WithName("UpdateEnergyThroughput")
                .ScheduleParallel();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach(
                    (ref EnergyStore energyStore, in Entity entity) =>
                    {
                        var nodeQuery = GetComponentDataFromEntity<Node>(true);
                        var internodeQuery = GetComponentDataFromEntity<Internode>(true);
                        var energyFlowQuery = GetComponentDataFromEntity<EnergyFlow>(true);

                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        energyStore.Capacity = 0;
                        if (nodeQuery.HasComponent(entity))
                        {
                            energyStore.Capacity += nodeQuery[entity].Volume;
                        }

                        if (internodeQuery.HasComponent(entity))
                        {
                            energyStore.Capacity += internodeQuery[entity].Volume;
                        }

                        energyStore.Capacity = math.max(0.001f, energyStore.Capacity);

                        if (energyFlowQuery.HasComponent(entity))
                        {
                            energyStore.Quantity += energyFlowQuery[entity].Throughput;
                        }

                        if (childrenQuery.HasComponent(entity))
                        {
                            var branches = childrenQuery[entity];

                            for (int i = 0; i < branches.Length; i++)
                            {
                                if (energyFlowQuery.HasComponent(branches[i].Value))
                                {
                                    energyStore.Quantity -= energyFlowQuery[branches[i].Value].Throughput;
                                }
                            }
                        }

                        energyStore.Quantity = math.clamp(energyStore.Quantity, 0, energyStore.Capacity);
                    })
                .WithName("UpdateEnergyQuantities")
                .ScheduleParallel();
        }
    }
}
