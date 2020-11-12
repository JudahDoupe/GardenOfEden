using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    public struct UpdateChunk : ISharedComponentData
    {
        public int Id;
        public float3 Position;
    }

    class UpdateChunkSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var writer = ecb.AsParallelWriter();
            var chunks = Singleton.LoadBalancer.UpdateChunks
                .Where(x => x.Id >= 0).ToList()
                .ToNativeArray(Allocator.TempJob);

            Entities
                .WithAll<UpdateChunk>()
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
                    writer.SetSharedComponent(entityInQueryIndex, entity, closestChunk);
                })
                .WithName("UpdateChunk")
                .WithDisposeOnCompletion(chunks)
                .ScheduleParallel(Dependency)
                .Complete();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}