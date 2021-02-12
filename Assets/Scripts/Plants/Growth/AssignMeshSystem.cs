using Unity.Entities;

namespace Assets.Scripts.Plants.Growth
{
    public struct AssignNodeMesh : IComponentData
    {
        public Entity Entity;
    }
    public struct AssignInternodeMesh : IComponentData
    {
        public Entity Entity;
    }

    public struct NodeMeshReference : IComponentData
    {
        public Entity Entity;
    }
    public struct InternodeMeshReference : IComponentData
    {
        public Entity Entity;
    }

    public struct NodeReference : IComponentData
    {
        public Entity Entity;
    }
    public struct InternodeReference : IComponentData
    {
        public Entity Entity;
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    class AssignMeshSystem : SystemBase
    {
        GrowthEcbSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<GrowthEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach(
                    (in AssignNodeMesh assignMesh, in Entity entity, in int entityInQueryIndex) =>
                    {
                        var refQuery = GetComponentDataFromEntity<NodeMeshReference>(true);

                        var meshEntity = ecb.Instantiate(entityInQueryIndex, assignMesh.Entity);

                        if (refQuery.HasComponent(entity))
                        {
                            ecb.DestroyEntity(entityInQueryIndex, refQuery[entity].Entity);
                        }
                        else
                        {
                            ecb.AddComponent<NodeMeshReference>(entityInQueryIndex, entity);
                        }

                        ecb.AddComponent<NodeReference>(entityInQueryIndex, meshEntity);
                        ecb.SetComponent(entityInQueryIndex, meshEntity, new NodeReference {Entity = entity}); 
                        ecb.SetComponent(entityInQueryIndex, entity, new NodeMeshReference { Entity = meshEntity });
                        ecb.RemoveComponent<AssignNodeMesh>(entityInQueryIndex, entity);
                    })
                .WithName("AddNodeMesh")
                .ScheduleParallel();

            Entities
                .WithNone<Dormant>()
                .ForEach(
                    (in AssignInternodeMesh assignMesh, in Entity entity, in int entityInQueryIndex) =>
                    {
                        var refQuery = GetComponentDataFromEntity<InternodeMeshReference>(true);

                        var meshEntity = ecb.Instantiate(entityInQueryIndex, assignMesh.Entity);

                        if (refQuery.HasComponent(entity))
                        {
                            ecb.DestroyEntity(entityInQueryIndex, refQuery[entity].Entity);
                        }
                        else
                        {
                            ecb.AddComponent<InternodeMeshReference>(entityInQueryIndex, entity);
                        }

                        ecb.AddComponent<InternodeReference>(entityInQueryIndex, meshEntity);
                        ecb.SetComponent(entityInQueryIndex, meshEntity, new InternodeReference { Entity = entity });
                        ecb.SetComponent(entityInQueryIndex, entity, new InternodeMeshReference { Entity = meshEntity });
                        ecb.RemoveComponent<AssignInternodeMesh>(entityInQueryIndex, entity);
                    })
                .WithName("AddInternodeMesh")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
