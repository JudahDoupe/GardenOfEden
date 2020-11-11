using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    public struct Chunk : ISharedComponentData
    {
        public int2 Id;
    }

    class UpdateChunkSystem : SystemBase, IDailyProcess
    {
        public const int ChunkSize = 10;

        public void ProcessDay(Action callback)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var writer = ecb.AsParallelWriter();
            var job = Entities
                .WithSharedComponentFilter(new Chunk())
                .WithNone<Dormant>()
                .ForEach((in Entity entity, in LocalToWorld l2w, in int entityInQueryIndex) =>
                {
                    var newId = GetChunkIdFromPosition(l2w.Position, ChunkSize);
                    writer.SetSharedComponent(entityInQueryIndex, entity, new Chunk { Id = newId });
                })
                .WithName("UpdateChunk")
                .ScheduleParallel(Dependency);

            job.Complete();
            ecb.Playback(EntityManager);
            ecb.Dispose();

            callback();
        }

        protected override void OnUpdate() { }

        public static int2 GetChunkIdFromPosition(float3 position, float chunkSize)
        {
            return math.int2(new float2(math.floor(position.x / chunkSize), math.floor(position.z / chunkSize)));
        }
    }
}