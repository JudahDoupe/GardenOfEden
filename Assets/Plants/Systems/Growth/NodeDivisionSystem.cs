using System;
using System.Linq;
using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Dna;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Growth
{
    [InternalBufferCapacity(4)]
    public struct DivisionInstruction : IBufferElementData
    {
        public Entity Entity;
        public Quaternion? Rotation;
        public LifeStage Stage;
        public DivisionOrder Order;
    }

    public struct NodeDivision : IComponentData
    {
        public LifeStage Stage;
        public int RemainingDivisions;
        public float MinEnergyPressure;
    }

    public enum LifeStage
    {
        Vegetation,
        Reproduction,
    }

    public enum DivisionOrder
    {
        InPlace,
        Replace,
        PreNode,
        PostNode,
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateAfter(typeof(GrowthSystem))]
    public class NodeDivisionSystem : SystemBase
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
            var genericSeed = new System.Random().Next();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref NodeDivision nodeDivision, ref EnergyStore energyStore, in Parent parent, in Entity entity, in int entityInQueryIndex) =>
                {
                    if (energyStore.Quantity / (energyStore.Capacity + float.Epsilon) < nodeDivision.MinEnergyPressure
                        || nodeDivision.RemainingDivisions < 0)
                        return;

                    var childrenQuery = GetBufferFromEntity<Child>(true);

                    var instructions = GetBufferFromEntity<DivisionInstruction>(true)[entity];
                    var currentNode = entity;
                    var hasDivided = false;
                    for (var i = 0; i < instructions.Length; i++)
                    {
                        var instruction = instructions[i];
                        if (instruction.Stage != nodeDivision.Stage)
                        {
                            continue;
                        }

                        hasDivided = true;
                        var seed = math.asuint((genericSeed * entityInQueryIndex + i) % uint.MaxValue) + 1;

                        var newNode = ecb.Instantiate(entityInQueryIndex, instruction.Entity);
                        var newRotation = instruction.Rotation ?? instruction.Order switch
                        {
                            DivisionOrder.Replace => GetComponentDataFromEntity<Rotation>(true)[currentNode].Value,
                            DivisionOrder.PreNode => GetComponentDataFromEntity<Rotation>(true)[currentNode].Value,
                            _ => Quaternion.identity
                        };
                        ecb.RemoveComponent<Dormant>(entityInQueryIndex, newNode);
                        ecb.SetSharedComponent(entityInQueryIndex, newNode, Singleton.LoadBalancer.CurrentChunk); 
                        ecb.SetComponent(entityInQueryIndex, newNode, new Rotation { Value = newRotation * RandomQuaternion(0.05f, seed) });

                        switch (instruction.Order)
                        {
                            case DivisionOrder.InPlace:
                                ecb.SetComponent(entityInQueryIndex, newNode, parent);
                                break;
                            case DivisionOrder.Replace:
                                if (childrenQuery.HasComponent(currentNode))
                                {
                                    var children = childrenQuery[currentNode];
                                    for (int c = 0; c < children.Length; c++)
                                    {
                                        ecb.SetComponent(entityInQueryIndex, children[c].Value, new Parent { Value = newNode });
                                        ecb.SetComponent(entityInQueryIndex, children[c].Value, new PreviousParent { Value = Entity.Null });
                                    }
                                    ecb.RemoveComponent<Child>(entityInQueryIndex, currentNode);
                                }
                                ecb.SetComponent(entityInQueryIndex, newNode, parent);
                                ecb.DestroyEntity(entityInQueryIndex, currentNode);
                                break;
                            case DivisionOrder.PreNode:
                                ecb.SetComponent(entityInQueryIndex, newNode, parent);
                                ecb.SetComponent(entityInQueryIndex, currentNode, new Parent { Value = newNode });
                                break;
                            case DivisionOrder.PostNode:
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = currentNode });
                                break;
                        }

                    }

                    nodeDivision.RemainingDivisions -= hasDivided ? 1 : 0;
                })
                .WithoutBurst()
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

