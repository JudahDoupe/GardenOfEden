using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    public struct UnparentDormancyTrigger : IComponentData { }
    public struct GrowthHormoneDormancyTrigger : IComponentData
    {
        public float MaxPressure;
        public float MinPressure;
    }

    public class DormancyTriggerSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {

            var ecb1 = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithAll<Dormant, UnparentDormancyTrigger>()
                .ForEach((in Entity entity, in int entityInQueryIndex) =>
                {
                    if (!GetComponentDataFromEntity<Parent>(true).HasComponent(entity))
                    {
                        ecb1.RemoveComponent<Dormant>(entityInQueryIndex, entity);
                    }
                })
                .WithName("UnparentDormancyTrigger")
                .ScheduleParallel();

            var ecb2 = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithAll<Dormant>()
                .ForEach((in GrowthHormoneDormancyTrigger trigger, in GrowthHormoneStore hormone, in Entity entity, in int entityInQueryIndex) =>
                {
                    if (trigger.MinPressure < hormone.Pressure && hormone.Pressure < trigger.MaxPressure)
                    {
                        ecb2.RemoveComponent<Dormant>(entityInQueryIndex, entity);
                    }
                })
                .WithName("GrowthHormoneDormancyTrigger")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

    }
}

