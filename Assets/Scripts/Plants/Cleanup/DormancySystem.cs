using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Cleanup
{
    public struct Dormant : IComponentData { }
    public struct ParentDormancyTrigger : IComponentData 
    {
        public bool IsDormantWhenParented;
        public bool IsDormantWhenUnparented;
    }

    [UpdateInGroup(typeof(CleanupSystemGroup))]
    public class DormancySystem : SystemBase
    {
        CleanupEcbSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<CleanupEcbSystem>();
        }

        protected override void OnUpdate()
        {

            var ecb1 = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach((in ParentDormancyTrigger trigger, in Entity entity, in int entityInQueryIndex) =>
                {
                    var isDormant = GetComponentDataFromEntity<Dormant>(true).HasComponent(entity);
                    var hasParent = GetComponentDataFromEntity<Parent>(true).HasComponent(entity);
                    var shouldBeDormant = (trigger.IsDormantWhenParented && hasParent) 
                                           || (trigger.IsDormantWhenUnparented && !hasParent);

                    if (!isDormant && shouldBeDormant)
                        ecb1.AddComponent<Dormant>(entityInQueryIndex, entity);

                    if (isDormant && !shouldBeDormant)
                        ecb1.RemoveComponent<Dormant>(entityInQueryIndex, entity);

                })
                .WithName("ParentDormancyTrigger")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

    }
}

