using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    public struct AssignNodeMesh : IComponentData
    {
        public FixedString64 MeshName { get; set; }
    }
    public struct AssignInternodeMesh : IComponentData
    {
        public FixedString64 MeshName { get; set; }
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

    class AssignMeshSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach(
                    (in AssignNodeMesh assignMesh, in Entity entity) =>
                    {
                        var refQuery = GetComponentDataFromEntity<NodeMeshReference>(true);

                        if (assignMesh.MeshName.Length > 0)
                        {
                            var mesh = Singleton.RenderMeshLibrary.Library[assignMesh.MeshName.ToString()];

                            Entity meshEntity;
                            if (refQuery.HasComponent(entity))
                            {
                                meshEntity = refQuery[entity].Entity;
                            }
                            else
                            {
                                meshEntity = ecb.CreateEntity(Singleton.RenderMeshLibrary.NodeMeshArchetype);
                                ecb.AddComponent(entity, typeof(NodeMeshReference));
                                ecb.SetComponent(entity, new NodeMeshReference { Entity = meshEntity });
                                ecb.SetComponent(meshEntity, new NodeReference { Entity = entity });
                            }

                            ecb.SetSharedComponent(meshEntity, mesh.Mesh);
                            ecb.SetComponent(meshEntity, mesh.Bounds);
                        }
                        else if (refQuery.HasComponent(entity))
                        {
                            var meshEntity = refQuery[entity].Entity;
                            ecb.RemoveComponent(entity, typeof(NodeMeshReference));
                            ecb.DestroyEntity(meshEntity);
                        }

                        ecb.RemoveComponent(entity, typeof(AssignNodeMesh));
                    })
                .WithoutBurst()
                .WithName("AddNodeMesh")
                .Run();

            Entities
                .WithNone<Dormant>()
                .ForEach(
                    (in AssignInternodeMesh assignMesh, in Entity entity) =>
                    {
                        var refQuery = GetComponentDataFromEntity<InternodeMeshReference>(true);

                        if (assignMesh.MeshName.Length > 0)
                        {
                            var mesh = Singleton.RenderMeshLibrary.Library[assignMesh.MeshName.ToString()];

                            Entity meshEntity;
                            if (refQuery.HasComponent(entity))
                            {
                                meshEntity = refQuery[entity].Entity;
                            }
                            else
                            {
                                meshEntity = ecb.CreateEntity(Singleton.RenderMeshLibrary.InternodeMeshArchetype);
                                ecb.AddComponent(entity, typeof(InternodeMeshReference));
                                ecb.SetComponent(entity, new InternodeMeshReference { Entity = meshEntity });
                                ecb.SetComponent(meshEntity, new InternodeReference { Entity = entity });
                            }

                            ecb.SetSharedComponent(meshEntity, mesh.Mesh);
                            ecb.SetComponent(meshEntity, mesh.Bounds);
                        }
                        else if (refQuery.HasComponent(entity))
                        {
                            var meshEntity = refQuery[entity].Entity;
                            ecb.RemoveComponent(entity, typeof(InternodeMeshReference));
                            ecb.DestroyEntity(meshEntity);
                        }

                        ecb.RemoveComponent(entity, typeof(AssignInternodeMesh));
                    })
                .WithoutBurst()
                .WithName("AddInternodeMesh")
                .Run();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
