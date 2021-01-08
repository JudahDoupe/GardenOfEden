using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    [MaximumChunkCapacity(4096)]
    public struct UpdateChunk : ISharedComponentData, IEquatable<UpdateChunk>
    {
        public int Id;
        public float3 Position;
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

    class UpdateChunkSystem : SystemBase
    {
        private List<UpdateChunk> _chunks = new List<UpdateChunk>();
        private UpdateChunk _currentChunk = new UpdateChunk();

        EndSimulationEntityCommandBufferSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            if (Singleton.LoadBalancer.CurrentChunk.Id != -1) return;

            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var chunks = Singleton.LoadBalancer.UpdateChunks
                .Where(x => x.Id >= 0).ToList()
                .ToNativeArray(Allocator.TempJob);
            var currentChunk = _currentChunk;

            Entities
                .WithSharedComponentFilter(_currentChunk)
                .WithNativeDisableParallelForRestriction(chunks)
                .ForEach((in Entity entity, in LocalToWorld l2w, in int entityInQueryIndex) =>
                {
                    var closestChunk = chunks[0];
                    for (int i = 0; i < chunks.Length; i++)
                    {
                        if (math.distance(chunks[i].Position, l2w.Position) <
                            math.distance(closestChunk.Position, l2w.Position))
                        {
                            closestChunk = chunks[i];
                        }
                    }

                    if (!currentChunk.Equals(closestChunk))
                    {
                        ecb.SetSharedComponent(entityInQueryIndex, entity, closestChunk);
                    }
                })
                .WithName("UpdateChunk")
                .WithDisposeOnCompletion(chunks)
                .WithoutBurst()
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);

            _chunks.Remove(_currentChunk);
            if (!_chunks.Any())
            {
                Singleton.LoadBalancer.BalanceChunks();
                EntityManager.GetAllUniqueSharedComponentData(_chunks);
            }
            _currentChunk = _chunks.First();
        }
    }
}