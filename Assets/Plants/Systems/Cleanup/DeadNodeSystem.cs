using System.Linq;
using Assets.Scripts.Plants.Growth;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Plants.Systems.Cleanup
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
                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        if (health.Value < 0)
                        {
                            DestroyAllChildren(entity, ecb, entityInQueryIndex, childrenQuery);
                        }
                    })
                .WithName("RemoveDeadNode")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

        public static void DestroyAllChildren(Entity e, EntityCommandBuffer.ParallelWriter ecb, int id, BufferFromEntity<Child> childrenQuery)
        {
            if (childrenQuery.HasComponent(e))
            {
                if (childrenQuery.HasComponent(e))
                {
                    var branches = childrenQuery[e];

                    for (int i = 0; i < branches.Length; i++)
                    {
                        DestroyAllChildren(branches[i].Value, ecb, id, childrenQuery);
                    }
                }
            }
            ecb.DestroyEntity(id, e);
        }
    }
}
