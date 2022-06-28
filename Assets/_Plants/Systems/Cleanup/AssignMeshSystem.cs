using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Plants.Systems.Cleanup
{
    public struct AssignNodeMesh : IComponentData
    {
        public Entity Node;
        public Entity Internode;
    }

    public struct NodeMeshReference : IComponentData
    {
        public Entity Node;
        public Entity Internode;
    }

    public struct NodeMesh : IComponentData
    {
        public Entity Node;
        public bool IsInternode;
    }

    [UpdateInGroup(typeof(CleanupSystemGroup))]
partial     class AssignMeshSystem : SystemBase
    {
        CleanupEcbSystem _ecbSystem;
        DeleteEntityEcbSystem _trashEcbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<CleanupEcbSystem>();
            _trashEcbSystem = World.GetOrCreateSystem<DeleteEntityEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var trashEcb = _trashEcbSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach(
                    (ref NodeMeshReference meshReference, in AssignNodeMesh assignMesh, in Parent parent, in Entity entity, in int entityInQueryIndex) =>
                    {
                        if (meshReference.Node != Entity.Null)
                        {
                            trashEcb.DestroyEntity(entityInQueryIndex, meshReference.Node);
                            meshReference.Node = Entity.Null;
                        }
                        if (meshReference.Internode != Entity.Null)
                        {
                            trashEcb.DestroyEntity(entityInQueryIndex, meshReference.Internode);
                            meshReference.Internode = Entity.Null;
                        }

                        if (assignMesh.Node != Entity.Null)
                        {
                            meshReference.Node = ecb.Instantiate(entityInQueryIndex, assignMesh.Node);
                            ecb.SetComponent(entityInQueryIndex, meshReference.Node, new Parent{ Value = entity });
                            ecb.SetComponent(entityInQueryIndex, meshReference.Node, new NodeMesh { IsInternode = false, Node = entity });
                        }
                        if (assignMesh.Internode != Entity.Null)
                        {
                            meshReference.Internode = ecb.Instantiate(entityInQueryIndex, assignMesh.Internode);
                            ecb.SetComponent(entityInQueryIndex, meshReference.Internode, new Parent { Value = parent.Value });
                            ecb.SetComponent(entityInQueryIndex, meshReference.Internode, new NodeMesh{ IsInternode = true, Node = entity });
                        }

                        ecb.RemoveComponent<AssignNodeMesh>(entityInQueryIndex, entity);
                    })
                .WithName("AddNodeMesh")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
