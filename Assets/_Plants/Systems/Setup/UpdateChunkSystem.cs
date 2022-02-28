using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Growth;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Setup
{
    [Serializable]
    [MaximumChunkCapacity(4096)]
    public struct UpdateChunk : ISharedComponentData, IEquatable<UpdateChunk>
    {
        public int Id;
        public bool IsEnvironmental => Id < 0;
        public bool Equals(UpdateChunk other) => other.Id == Id;
        public override bool Equals(object other) => other is UpdateChunk chunk && chunk.Id == Id;
        public override int GetHashCode() => Id;
        public static bool operator == (UpdateChunk lhs, UpdateChunk rhs) => lhs.Equals(rhs);
        public static bool operator != (UpdateChunk lhs, UpdateChunk rhs) => !(lhs.Equals(rhs));
    }

    [UpdateInGroup(typeof(SetupSystemGroup), OrderFirst = true)]
    class UpdateChunkSystem : SystemBase
    {
        SetupEcbSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<SetupEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var planet = Planet.Entity;
            var position = Singleton.LoadBalancer.Position;
            var radius = Singleton.LoadBalancer.Radius;
            var activeChunk = Singleton.LoadBalancer.ActiveEntityChunk;
            var inactiveChunk = Singleton.LoadBalancer.InactiveEntityChunk;

            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(inactiveChunk)
                .ForEach((in Entity entity, in Coordinate coord, in int entityInQueryIndex) =>
                {
                    var l2w = GetComponent<LocalToWorld>(planet);
                    var childrenQuery = GetBufferFromEntity<Child>(true);
                    var nodeQuery = GetComponentDataFromEntity<Node>(true);
                    if (math.distance(coord.Global(l2w), position) <= radius)
                    {
                        RecursivelySetChunk(entity, childrenQuery, activeChunk, nodeQuery, ecb, entityInQueryIndex);
                    }
                })
                .WithoutBurst()
                .WithName("UpdateActiveChunk")
                .ScheduleParallel();

            var ecb2 = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(activeChunk)
                .ForEach((in Entity entity, in Coordinate coord, in int entityInQueryIndex) =>
                {
                    var l2w = GetComponent<LocalToWorld>(planet);
                    var childrenQuery = GetBufferFromEntity<Child>(true);
                    var nodeQuery = GetComponentDataFromEntity<Node>(true);
                    if (math.distance(coord.Global(l2w), position) > radius)
                    {
                        RecursivelySetChunk(entity, childrenQuery, inactiveChunk, nodeQuery, ecb2, entityInQueryIndex);
                    }
                })
                .WithoutBurst()
                .WithName("UpdateInactiveChunk")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

        private static void RecursivelySetChunk(Entity entity, BufferFromEntity<Child> childrenQuery, UpdateChunk chunk, ComponentDataFromEntity<Node> nodeQuery, EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex)
        {
            if (nodeQuery.HasComponent(entity))
            {
                ecb.SetSharedComponent(entityInQueryIndex, entity, chunk);
            }

            if (childrenQuery.HasComponent(entity))
            {
                var children = childrenQuery[entity];

                for (int i = 0; i < children.Length; i++)
                {
                    RecursivelySetChunk(children[i].Value, childrenQuery, chunk, nodeQuery, ecb, entityInQueryIndex); 
                }
            }
        }
    }
}