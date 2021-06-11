using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Growth
{
    public struct GrowthHormoneStore : IComponentData
    {
        public float Quantity;
        public float Capacity;
        public float Pressure => Quantity / (Capacity + float.Epsilon);
    }

    public struct GrowthHormoneFlow : IComponentData
    {
        public float Throughput;
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateBefore(typeof(GrowthSystem))]
    public class GrowthHormoneSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach(
                    (ref GrowthHormoneFlow hormoneFlow, in GrowthHormoneStore hormoneStore, in Entity entity) =>
                    {
                        var hormoneStoreQuery = GetComponentDataFromEntity<GrowthHormoneStore>(true);
                        var parentQuery = GetComponentDataFromEntity<Parent>(true);
                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        if (!parentQuery.HasComponent(entity)
                            || parentQuery[entity].Value == Entity.Null
                            || !hormoneStoreQuery.HasComponent(parentQuery[entity].Value)
                            || !childrenQuery.HasComponent(parentQuery[entity].Value))
                        {
                            hormoneFlow.Throughput = 0;
                        }
                        else
                        {
                            var headStore = hormoneStore;
                            var tailStore = hormoneStoreQuery[parentQuery[entity].Value];
                            var numBranches = childrenQuery[parentQuery[entity].Value].Length + 1;

                            var resistance = 0f;
                            var flowRate = (1f / numBranches) / (1 + resistance);

                            var greaterQuantity = tailStore.Pressure > headStore.Pressure ? tailStore.Quantity : headStore.Quantity;
                            hormoneFlow.Throughput = flowRate * greaterQuantity * (tailStore.Pressure - headStore.Pressure);
                        }
                    })
                .WithName("UpdateGrowthHormoneThroughput")
                .ScheduleParallel();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach(
                    (ref GrowthHormoneStore hormoneStore, in Entity entity) =>
                    {
                        var nodeQuery = GetComponentDataFromEntity<Node>(true);
                        var hormoneFlowQuery = GetComponentDataFromEntity<GrowthHormoneFlow>(true);

                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        if (nodeQuery.HasComponent(entity))
                        {
                            hormoneStore.Capacity = math.max(0.001f, nodeQuery[entity].Volume);
                        }

                        if (hormoneFlowQuery.HasComponent(entity))
                        {
                            hormoneStore.Quantity += hormoneFlowQuery[entity].Throughput;
                        }

                        if (childrenQuery.HasComponent(entity))
                        {
                            var branches = childrenQuery[entity];

                            for (int i = 0; i < branches.Length; i++)
                            {
                                if (hormoneFlowQuery.HasComponent(branches[i].Value))
                                {
                                    hormoneStore.Quantity -= hormoneFlowQuery[branches[i].Value].Throughput;
                                }
                            }
                        }

                        hormoneStore.Quantity = math.clamp(hormoneStore.Quantity, 0, hormoneStore.Capacity);
                    })
                .WithName("UpdateGrowthHormoneQuantities")
                .ScheduleParallel();
        }
    }
}
