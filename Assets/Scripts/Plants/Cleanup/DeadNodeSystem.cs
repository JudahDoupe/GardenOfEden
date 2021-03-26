using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Cleanup
{

    [UpdateInGroup(typeof(CleanupSystemGroup))]
    public class DeadNodeSystem : SystemBase
    {
        CleanupEcbSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<CleanupEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach(
                    (ref Health health, in Entity entity, in int entityInQueryIndex) =>
                    {
                        var nodeMesh = GetComponentDataFromEntity<NodeMeshReference>(true);
                        var internodeMesh = GetComponentDataFromEntity<InternodeMeshReference>(true);
                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        if (health.Value < 0 && !childrenQuery.HasComponent(entity))
                        {
                            if (nodeMesh.HasComponent(entity))
                            {
                                ecb.DestroyEntity(entityInQueryIndex, nodeMesh[entity].Entity);
                            }

                            if (internodeMesh.HasComponent(entity))
                            {
                                ecb.DestroyEntity(entityInQueryIndex, internodeMesh[entity].Entity);
                            }

                            ecb.DestroyEntity(entityInQueryIndex, entity);
                        }
                    })
                .WithName("RemoveDeadNode")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
