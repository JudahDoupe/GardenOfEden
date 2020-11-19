using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Systems
{
    public struct Dormant : IComponentData { }

    [InternalBufferCapacity(8)]
    public struct EmbryoNode : IBufferElementData
    {
        public Entity Entity;
        public Quaternion Rotation;
        public NodeType Type;
        public DivisionOrder Order;
    }

    public struct DnaReference : IComponentData
    {
        public Entity Entity;
    }

    public struct NodeDivision : IComponentData
    {
        public NodeType Type;
        public int RemainingDivisions;
    }

    public enum DivisionOrder
    {
        InPlace,
        Replace,
        PreNode,
        PostNode,
    }
    public enum NodeType
    {
        Vegetation,
        Reproduction,
        Embryo,
        Seedling,
    }

    public class NodeDivisionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() 
        {
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var genericSeed = new System.Random().Next();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref NodeDivision nodeDivision, ref EnergyStore energyStore, in DnaReference dnaRef,
                    in Entity entity, in int entityInQueryIndex) =>
                {
                    if (energyStore.Quantity / (energyStore.Capacity + float.Epsilon) < 0.5f
                        || nodeDivision.RemainingDivisions < 0)
                        return;

                    var parentQuery = GetComponentDataFromEntity<Parent>(true);
                    var embryoNodes = GetBufferFromEntity<EmbryoNode>(true)[dnaRef.Entity];

                    for (var i = 0; i < embryoNodes.Length; i++)
                    {
                        var embryo = embryoNodes[i];
                        if (embryo.Type != nodeDivision.Type)
                        {
                            continue;
                        }

                        var seed = math.asuint((genericSeed * entityInQueryIndex + i) % uint.MaxValue) + 1;
                        var parent = parentQuery.HasComponent(entity) ? parentQuery[entity].Value : Entity.Null;
                        var newNode = ecb.Instantiate(entityInQueryIndex, embryo.Entity);
                        ecb.SetComponent(entityInQueryIndex, newNode,
                            new Rotation { Value = embryo.Rotation * RandomQuaternion(0.05f, seed) });
                        if (nodeDivision.Type != NodeType.Embryo)
                        {
                            ecb.RemoveComponent<Dormant>(entityInQueryIndex, newNode);
                        }

                        switch (embryo.Order)
                        {
                            case DivisionOrder.InPlace:
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = parent });
                                break;
                            case DivisionOrder.Replace:
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = parent });
                                ecb.DestroyEntity(entityInQueryIndex, entity);
                                break;
                            case DivisionOrder.PreNode:
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = parent });
                                ecb.SetComponent(entityInQueryIndex, entity, new Parent { Value = newNode });
                                break;
                            case DivisionOrder.PostNode:
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = entity });
                                break;
                        }

                    }

                    nodeDivision.RemainingDivisions--;
                })
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

        private static Quaternion RandomQuaternion(float maxAngle, uint seed)
        {
            var rand = new Unity.Mathematics.Random(seed);
            var rtn = rand.NextFloat3() % maxAngle * 2;
            rtn -= new float3(maxAngle);
            return new Quaternion(rtn.x, rtn.y, rtn.z, 1);
        }

    }
}

