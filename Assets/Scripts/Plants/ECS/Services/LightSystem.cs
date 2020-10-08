using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Assets.Scripts.Plants.ECS.Services.TransportationSystems;
using Unity.Collections;
using Assets.Scripts.Plants.ECS.Components;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS.Services
{
    public struct InsertLightAbsorber : IComponentData { }

    public struct LightAbsorber : IComponentData
    {
        public float AbsorbedLight { get; set; }
        public float SurfaceArea { get; set; }
    }

    public struct LightLevel : IComponentData
    {
        public Entity ParentLevel { get; set; }
        public float AvailableLight { get; set; }
    }

    public struct Choloplast : IComponentData 
    { 
        public float Efficiency { get; set; }
    }


    class LightSystem : SystemBase
    {
        private static int WorldSize = 200;
        private static float ColumnDensity = 1f;
        private static NativeHashMap<int2, Entity> LightColumns;

        protected override void OnCreate()
        {
            base.OnCreate();
            LightColumns = new NativeHashMap<int2, Entity>(WorldSize * WorldSize, Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<LocalToWorld>()
                .ForEach((ref LightAbsorber absorber, in Entity entity, in LocalToWorld l2w, in RenderBounds bounds) =>
                {
                    var internodeQuerry = GetComponentDataFromEntity<Internode>(true);
                    var scaleQuery = GetComponentDataFromEntity<Scale>(true);
                    var scaleQuery2 = GetComponentDataFromEntity<NonUniformScale>(true);
                    
                    if (internodeQuerry.HasComponent(entity))
                    {
                        var angle = 1 - math.abs(math.dot(l2w.Forward, new float3(0, 1, 0)));
                        var internode = internodeQuerry[entity];
                        absorber.SurfaceArea = internode.Length * internode.Radius * angle;
                    }
                    else if (scaleQuery.HasComponent(entity))
                    {
                        var extents = bounds.Value.Extents * scaleQuery[entity].Value;
                        var globalExtents = math.mul(l2w.Rotation, extents);
                        absorber.SurfaceArea = globalExtents.x * globalExtents.z;
                    }
                    else if (scaleQuery2.HasComponent(entity))
                    {
                        var extents = bounds.Value.Extents * scaleQuery2[entity].Value;
                        var globalExtents = math.mul(l2w.Rotation, extents);
                        absorber.SurfaceArea = globalExtents.x * globalExtents.z;
                    }
                    else
                    {
                        absorber.SurfaceArea = 0;
                    }


                })
                .WithName("UpdateSurfaceArea")
                .ScheduleParallel();

            EntityQuery lightAbsorberEntityQuery = GetEntityQuery(typeof(LightAbsorber), typeof(LightLevel));
            NativeArray<float> lightLevels = new NativeArray<float>(lightAbsorberEntityQuery.CalculateEntityCount(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            //TODO: What happens when a light absorber moves to a differnet column

            var hadle = Entities
                .WithNativeDisableParallelForRestriction(lightLevels)
                .ForEach((int entityInQueryIndex, ref LightAbsorber absorber, in LightLevel lightLevel) =>
                {
                    var cellQuery = GetComponentDataFromEntity<LightLevel>(true);
                    var availableLight = 1f;
                    if (cellQuery.HasComponent(lightLevel.ParentLevel))
                    {
                        availableLight = cellQuery[lightLevel.ParentLevel].AvailableLight;
                    }

                    absorber.AbsorbedLight = math.min(availableLight, absorber.SurfaceArea);
                    lightLevels[entityInQueryIndex] = math.max(availableLight - absorber.AbsorbedLight, 0);
                })
                .WithName("UpdateAbsorbedLight")
                .WithBurst()
                .ScheduleParallel(Dependency);

            hadle.Complete();

            Entities
                .WithReadOnly(lightLevels)
                .WithDisposeOnCompletion(lightLevels)
                .WithAll<LightAbsorber>()
                .ForEach((int entityInQueryIndex, ref LightLevel lightLevel) =>
                {
                    lightLevel.AvailableLight = lightLevels[entityInQueryIndex];
                })
                .WithName("UpdateAvailableLight")
                .ScheduleParallel();

            Entities
                .ForEach((ref EnergyStore energyStore, in LightAbsorber absorber, in Choloplast chloroplast) =>
                {
                    Debug.Log("Woo");
                    energyStore.Quantity += absorber.AbsorbedLight * chloroplast.Efficiency;
                })
                .WithName("Photosynthesis")
                .WithBurst()
                .ScheduleParallel();

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var handle = Entities
                .WithAll<InsertLightAbsorber>()
                .ForEach((in Entity entity, in LocalToWorld l2w) =>
                {
                    var lightLevelQuery = GetComponentDataFromEntity<LightLevel>(true);
                    var localToWorldQuery = GetComponentDataFromEntity<LocalToWorld>(true);
                    var cellId = GetCellIdFromPosition(l2w.Position);

                    Entity upperEntity = Entity.Null;
                    Entity lowerEntity = Entity.Null;
                    if (LightColumns.TryGetValue(cellId, out upperEntity))
                    {
                        var nextEntity = lightLevelQuery[upperEntity].ParentLevel;
                        var nextHeight = nextEntity == Entity.Null ? float.MaxValue : localToWorldQuery[nextEntity].Position.y;
                        while (upperEntity != Entity.Null && nextHeight > l2w.Position.y)
                        {
                            lowerEntity = upperEntity;
                            upperEntity = nextEntity;
                            nextEntity = lightLevelQuery[upperEntity].ParentLevel;
                            nextHeight = nextEntity == Entity.Null ? float.MaxValue : localToWorldQuery[nextEntity].Position.y;
                        }

                        ecb.AddComponent(entity, new LightLevel { ParentLevel = upperEntity });
                        ecb.SetComponent(lowerEntity, new LightLevel { ParentLevel = entity });
                    }
                    else
                    {
                        LightColumns.Add(cellId, entity);
                        ecb.AddComponent(entity, new LightLevel { ParentLevel = Entity.Null });
                    }

                    ecb.RemoveComponent(entity, typeof(InsertLightAbsorber));
                    ecb.AddComponent(entity, typeof(LightAbsorber));
                })
                .WithName("InsertLightAbsorber")
                .Schedule(Dependency);
            handle.Complete();
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        public static int2 GetCellIdFromPosition(float3 position)
        {
            return math.int2(new float2(math.floor(position.x / ColumnDensity), math.floor(position.z / ColumnDensity)));
        }

    }
}

