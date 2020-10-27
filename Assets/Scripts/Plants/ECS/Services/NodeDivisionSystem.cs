using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS.Services
{
    public struct Dormant : IComponentData { }

    [InternalBufferCapacity(8)]
    public struct NodeDivision : IBufferElementData
    {
        public Entity Entity;
        public Quaternion Rotation;
        public DivisionOrder Order;
        public int NumDivisions;
    }

    public enum DivisionOrder
    {
        InPlace,
        PreNode,
        PostNode,
    }

    public class NodeDivisionSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var writer = ecb.AsParallelWriter();

            var job = Entities
                .WithNone<Dormant>()
                .ForEach((ref DynamicBuffer<NodeDivision> embryoNodes, ref EnergyStore energyStore, in Entity entity, in int entityInQueryIndex) =>
                {
                    if (energyStore.Quantity / (energyStore.Capacity + float.Epsilon) < 0.5f)
                        return;

                    var parentQuery = GetComponentDataFromEntity<Parent>(true);

                    for (var i = 0; i < embryoNodes.Length; i++)
                    {
                        var embryo = embryoNodes[i];
                        if (embryo.NumDivisions == 0)
                        {
                            embryoNodes.RemoveAt(i);
                            continue;
                        }

                        var parent = parentQuery.HasComponent(entity) ? parentQuery[entity].Value : Entity.Null;
                        var newNode = writer.Instantiate(entityInQueryIndex, embryo.Entity);
                        writer.SetComponent(entityInQueryIndex, newNode, new Rotation {Value = embryo.Rotation});
                        writer.RemoveComponent<Dormant>(entityInQueryIndex, newNode);
                        switch (embryo.Order)
                        {
                            case DivisionOrder.InPlace:
                                writer.SetComponent(entityInQueryIndex, newNode, new Parent {Value = parent});
                                break;
                            case DivisionOrder.PreNode:
                                writer.SetComponent(entityInQueryIndex, newNode, new Parent {Value = parent});
                                writer.SetComponent(entityInQueryIndex, entity, new Parent {Value = newNode});
                                break;
                            case DivisionOrder.PostNode:
                                writer.SetComponent(entityInQueryIndex, newNode, new Parent {Value = entity});
                                break;
                        }

                        embryo.NumDivisions--;
                        embryoNodes[i] = embryo;
                    }

                    if (embryoNodes.Length == 0)
                    {
                        embryoNodes.Clear();
                    }
                })
                .WithBurst()
                .ScheduleParallel(Dependency);

            job.Complete();
            ecb.Playback(EntityManager);
        }

    }
}

