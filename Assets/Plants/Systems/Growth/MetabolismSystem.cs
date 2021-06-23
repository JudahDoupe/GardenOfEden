using Assets.Plants.Systems.Cleanup;
using Unity.Entities;

namespace Assets.Scripts.Plants.Growth
{
    public struct Health : IComponentData
    {
        public float Value;
    }

    public struct Metabolism : IComponentData
    {
        public float Resting;
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateAfter(typeof(PhotosynthesisSystem))]
    public class MetabolismSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref EnergyStore energyStore, ref Health health, in Metabolism metabolism, in Entity entity) =>
                {
                    var nodeQuery = GetComponentDataFromEntity<Node>(true);
                    if (nodeQuery.HasComponent(entity))
                    {
                        energyStore.Quantity -= nodeQuery[entity].Volume * metabolism.Resting;
                    }

                    if (energyStore.Quantity < 0)
                    {
                        health.Value += energyStore.Quantity;
                        energyStore.Quantity = 0;
                    }
                })
                .WithName("Metabolism")
                .ScheduleParallel();
        }

    }
}

