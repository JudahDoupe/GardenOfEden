﻿using System;
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

    class AssignMeshSystem : SystemBase, IDailyProcess
    {
        public void ProcessDay(Action callback)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            Entities
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
                                meshEntity = ecb.CreateEntity();
                                ecb.AddComponent(entity, typeof(NodeMeshReference));
                                ecb.SetComponent(entity, new NodeMeshReference { Entity = meshEntity });
                                ecb.AddComponent(meshEntity, typeof(NodeReference));
                                ecb.SetComponent(meshEntity, new NodeReference { Entity = entity });
                                ecb.AddComponent(meshEntity, typeof(RenderMesh));
                                ecb.AddComponent(meshEntity, typeof(RenderBounds));
                                ecb.AddComponent(meshEntity, typeof(Translation));
                                ecb.AddComponent(meshEntity, typeof(Rotation));
                                ecb.AddComponent(meshEntity, typeof(NonUniformScale));
                                ecb.AddComponent(meshEntity, typeof(LocalToWorld));
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
                                meshEntity = ecb.CreateEntity();
                                ecb.AddComponent(entity, typeof(InternodeMeshReference));
                                ecb.SetComponent(entity, new InternodeMeshReference { Entity = meshEntity });
                                ecb.AddComponent(meshEntity, typeof(InternodeReference));
                                ecb.SetComponent(meshEntity, new InternodeReference { Entity = entity });
                                ecb.AddComponent(meshEntity, typeof(RenderMesh));
                                ecb.AddComponent(meshEntity, typeof(RenderBounds));
                                ecb.AddComponent(meshEntity, typeof(Translation));
                                ecb.AddComponent(meshEntity, typeof(Rotation));
                                ecb.AddComponent(meshEntity, typeof(NonUniformScale));
                                ecb.AddComponent(meshEntity, typeof(LocalToWorld));
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

            ecb.Playback(EntityManager);
            ecb.Dispose();
            callback();
        }

        protected override void OnUpdate() { }

    }
}
