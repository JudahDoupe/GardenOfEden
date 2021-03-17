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
    [MaximumChunkCapacity(4096)]
    public struct UpdateChunk : ISharedComponentData, IEquatable<UpdateChunk>
    {
        public int Id;
        public float3 Position;
        public float Radius;
        public bool IsEnvironmental => Id < 0;

        public bool Equals(UpdateChunk other)
        {
            return other.Id == Id;
        }
        public override bool Equals(object compare)
        {
            return compare is UpdateChunk uc && uc.Equals(this);
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public static bool operator ==(UpdateChunk lhs, UpdateChunk rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(UpdateChunk lhs, UpdateChunk rhs)
        {
            return !(lhs.Equals(rhs));
        }
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
            var activeChunk = Singleton.LoadBalancer.ActiveEntityChunk;
            var inactiveChunk = Singleton.LoadBalancer.InactiveEntityChunk;

            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(inactiveChunk)
                .ForEach((in Entity entity, in LocalToWorld l2w, in int entityInQueryIndex) =>
                {
                    if (math.distance(l2w.Position, activeChunk.Position) <= activeChunk.Radius)
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
                    if (math.distance(l2w.Position, activeChunk.Position) > activeChunk.Radius)
                    {
                        ecb2.SetSharedComponent(entityInQueryIndex, entity, inactiveChunk);
                    }
                })
                .WithoutBurst() 
                .WithName("UpdateInactiveChunk")
                .ScheduleParallel();

            var ecb3 = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithSharedComponentFilter(new UpdateChunk{Id = 0})
                .ForEach((in Entity entity, in LocalToWorld l2w, in int entityInQueryIndex) =>
                {
                    ecb3.SetSharedComponent(entityInQueryIndex, entity,
                        math.distance(l2w.Position, activeChunk.Position) > activeChunk.Radius
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