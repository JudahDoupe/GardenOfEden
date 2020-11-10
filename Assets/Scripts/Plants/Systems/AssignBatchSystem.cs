using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Plants.Systems
{
    public struct Batch : ISharedComponentData
    {
        public int Id;
    }

    class AssignBatchSystem : SystemBase, IDailyProcess
    {
        public const int EntitiesPerBatch = 100;
        private int _currentId = 1;

        public bool HasDayBeenProccessed() => true;

        public void ProcessDay()
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var writer = ecb.AsParallelWriter();

            var entityCount = GetEntitiesInBatch(0);
            var batches = new List<Batch>();
            EntityManager.GetAllUniqueSharedComponentData(batches);
            var batchIds = GetBatchIds(entityCount);

            var job = Entities
                .WithReadOnly(batchIds)
                .WithSharedComponentFilter(new Batch { Id = 0 })
                .WithNone<Dormant>()
                .ForEach((in Entity entity, in int entityInQueryIndex) =>
                {
                    writer.SetSharedComponent(entityInQueryIndex, entity, new Batch { Id = batchIds[entityInQueryIndex] });
                })
                .WithName("AssignBatch")
                .WithDisposeOnCompletion(batchIds)
                .ScheduleParallel(Dependency);

            job.Complete();
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        protected override void OnUpdate() { }


        private NativeArray<int> GetBatchIds(int count)
        {
            var batchIdArray = new NativeArray<int>(count, Allocator.TempJob);
            var remainingSpots = math.max(EntitiesPerBatch - GetEntitiesInBatch(_currentId), 0);
            for (int i = 0; i < count; i++)
            {
                while (remainingSpots <= 0)
                {
                    _currentId++;
                    remainingSpots = math.max(EntitiesPerBatch - GetEntitiesInBatch(_currentId), 0);
                }

                batchIdArray[i] = _currentId;
                remainingSpots--;
            }

            return batchIdArray;
        }

        private int GetEntitiesInBatch(int id)
        {
            var query = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(Dormant) },
                All = new ComponentType[] { typeof(Batch) },
            });
            query.ResetFilter();
            query.AddSharedComponentFilter(new Batch { Id = id });
            return query.CalculateEntityCount();
        }
    }
}
