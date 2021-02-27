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
                    var requiredEnergy = 0f;

                    var nodeQuery = GetComponentDataFromEntity<Node>(true);
                    requiredEnergy += nodeQuery.HasComponent(entity) ? nodeQuery[entity].Volume * metabolism.Resting : 0;
                    
                    var internodeQuery = GetComponentDataFromEntity<Node>(true);
                    requiredEnergy += internodeQuery.HasComponent(entity) ? internodeQuery[entity].Volume * metabolism.Resting : 0;

                    energyStore.Quantity -= requiredEnergy;
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

