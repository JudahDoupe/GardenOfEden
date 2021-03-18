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
            var position = Singleton.LoadBalancer.Position;
            var radius = Singleton.LoadBalancer.Radius;
            var activeChunk = Singleton.LoadBalancer.ActiveEntityChunk;
            var inactiveChunk = Singleton.LoadBalancer.InactiveEntityChunk;

            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(inactiveChunk)
                .ForEach((in Entity entity, in LocalToWorld l2w, in int entityInQueryIndex) =>
                {
                    if (math.distance(l2w.Position, position) <= radius)
                    {
                        ecb.SetSharedComponent(entityInQueryIndex, entity, activeChunk);
                    }
                })
                .WithoutBurst()
                .WithName("UpdateActiveChunk")
                .ScheduleParallel();

            var ecb2 = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(activeChunk)
                .ForEach((in Entity entity, in LocalToWorld l2w, in int entityInQueryIndex) =>
                {
                    if (math.distance(l2w.Position, position) > radius)
                    {
                        ecb2.SetSharedComponent(entityInQueryIndex, entity, inactiveChunk);
                    }
                })
                .WithoutBurst()
                .WithName("UpdateInactiveChunk")
                .ScheduleParallel();

            var ecb3 = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(new UpdateChunk())
                .ForEach((in Entity entity, in LocalToWorld l2w, in int entityInQueryIndex) =>
                {
                    ecb3.SetSharedComponent(entityInQueryIndex, entity,
                        math.distance(l2w.Position, position) > radius
                            ? inactiveChunk
                            : activeChunk);
                })
                .WithoutBurst()
                .WithName("SetChunk")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}