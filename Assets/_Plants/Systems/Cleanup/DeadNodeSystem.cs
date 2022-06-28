using System.Linq;
using Assets.Scripts.Plants.Growth;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Plants.Systems.Cleanup
{

    [UpdateInGroup(typeof(CleanupSystemGroup))]
    public partial class DeadNodeSystem : SystemBase
    {
        DeleteEntityEcbSystem _trashEcbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _trashEcbSystem = World.GetOrCreateSystem<DeleteEntityEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var trashEcb = _trashEcbSystem.CreateCommandBuffer().AsParallelWriter();

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
                            DestroyAllChildren(entity, trashEcb, entityInQueryIndex, childrenQuery);
                        }
                    })
                .WithName("RemoveDeadNode")
                .ScheduleParallel();

            _trashEcbSystem.AddJobHandleForProducer(Dependency);
        }

        public static void DestroyAllChildren(Entity e, EntityCommandBuffer.ParallelWriter trashEcb, int id, BufferFromEntity<Child> childrenQuery)
        {
            if (childrenQuery.HasComponent(e))
            {
                if (childrenQuery.HasComponent(e))
                {
                    var branches = childrenQuery[e];

                    for (int i = 0; i < branches.Length; i++)
                    {
                        DestroyAllChildren(branches[i].Value, trashEcb, id, childrenQuery);
                    }
                }
            }
            trashEcb.DestroyEntity(id, e);
        }
    }
}
