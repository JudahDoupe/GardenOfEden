using Assets.Scripts.Plants.Cleanup;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Growth
{
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
        public float MinEnergyPressure;
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
                .ForEach((ref NodeDivision nodeDivision, ref EnergyStore energyStore, in DnaReference dnaRef,
                    in Entity entity, in int entityInQueryIndex) =>
                {
                    if (energyStore.Quantity / (energyStore.Capacity + float.Epsilon) < nodeDivision.MinEnergyPressure
                        || nodeDivision.RemainingDivisions < 0)
                        return;

                    var parentQuery = GetComponentDataFromEntity<Parent>(true);
                    var childrenQuery = GetBufferFromEntity<Child>(true);
                    var embryoNodes = GetBufferFromEntity<EmbryoNode>(true)[dnaRef.Entity];

                    var parentNode = parentQuery.HasComponent(entity) ? parentQuery[entity].Value : Entity.Null;
                    var currentNode = entity;
                    for (var i = 0; i < embryoNodes.Length; i++)
                    {
                        var embryo = embryoNodes[i];
                        if (embryo.Type != nodeDivision.Type)
                        {
                            continue;
                        }

                        var seed = math.asuint((genericSeed * entityInQueryIndex + i) % uint.MaxValue) + 1;

                        var newNode = ecb.Instantiate(entityInQueryIndex, embryo.Entity); 
                        ecb.RemoveComponent<Dormant>(entityInQueryIndex, newNode);
                        ecb.SetSharedComponent(entityInQueryIndex, newNode, Singleton.LoadBalancer.ActiveEntityChunk);

                        switch (embryo.Order)
                        {
                            case DivisionOrder.InPlace:
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = parentNode });
                                ecb.SetComponent(entityInQueryIndex, newNode, new Rotation { Value = embryo.Rotation * RandomQuaternion(0.05f, seed) });
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
                                ecb.DestroyEntity(entityInQueryIndex, currentNode);
                                ecb.SetComponent(entityInQueryIndex, newNode, GetComponentDataFromEntity<Rotation>(true)[currentNode]); 
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = parentNode });
                                break;
                            case DivisionOrder.PreNode:
                                if (!parentQuery.HasComponent(currentNode)) ecb.AddComponent<Parent>(entityInQueryIndex, currentNode);
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = parentNode });
                                ecb.SetComponent(entityInQueryIndex, currentNode, new Parent { Value = newNode });
                                ecb.SetComponent(entityInQueryIndex, newNode, GetComponentDataFromEntity<Rotation>(true)[currentNode]);
                                break;
                            case DivisionOrder.PostNode:
                                ecb.SetComponent(entityInQueryIndex, newNode, new Parent { Value = currentNode });
                                ecb.SetComponent(entityInQueryIndex, newNode, new Rotation { Value = embryo.Rotation * RandomQuaternion(0.05f, seed) });
                                break;
                        }

                    }

                    nodeDivision.RemainingDivisions--;
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

