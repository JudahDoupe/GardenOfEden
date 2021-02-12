using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Environment
{
    public struct LightAbsorption : IComponentData
    {
        public float SurfaceArea;
        public int2 CellId;
    }

    public struct Photosynthesis : IComponentData
    {
        public float Efficiency;
    }

    [UpdateInGroup(typeof(EnvironmentSystemGroup))]
    public class LightSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float cellSize = 5;

            var query = GetEntityQuery(typeof(LightAbsorption), typeof(LocalToWorld), typeof(UpdateChunk));
            query.ResetFilter();
            query.AddSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk);
            var entityCount = query.CalculateEntityCount();
            if(entityCount == 0) return;

            var lightCells = new NativeMultiHashMap<int2, Entity>(entityCount, Allocator.TempJob);
            var lightCellsWriter = lightCells.AsParallelWriter();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach((ref LightAbsorption absorber, in Entity entity, in LocalToWorld l2w) =>
                {
                    var internodeQuery = GetComponentDataFromEntity<Internode>(true);
                    var nodeQuery = GetComponentDataFromEntity<Node>(true);

                    absorber.SurfaceArea = 0;
                    if (internodeQuery.HasComponent(entity))
                    {
                        var internode = internodeQuery[entity];
                        var globalSize = math.mul(l2w.Rotation,
                            new float3(internode.Radius, internode.Radius, internode.Length));
                        absorber.SurfaceArea += math.abs(globalSize.x * globalSize.z);
                    }

                    if (nodeQuery.HasComponent(entity))
                    {
                        var node = nodeQuery[entity];
                        var globalSize = math.mul(l2w.Rotation, node.Size);
                        absorber.SurfaceArea += math.abs(globalSize.x * globalSize.z);
                    }

                    absorber.CellId = GetCellIdFromPosition(l2w.Position, cellSize);
                    lightCellsWriter.Add(absorber.CellId, entity);

                })
                .WithName("UpdateLightAbsorber")
                .ScheduleParallel();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNativeDisableParallelForRestriction(lightCells)
                .ForEach((ref EnergyStore energyStore, in LightAbsorption absorber, in LocalToWorld l2w, in Photosynthesis photosynthesis) =>
                {
                    var l2wQuery = GetComponentDataFromEntity<LocalToWorld>(true);
                    var lightQuery = GetComponentDataFromEntity<LightAbsorption>(true);

                    var availableLight = cellSize * cellSize;

                    var absorbers = lightCells.GetValuesForKey(absorber.CellId);
                    while (absorbers.MoveNext())
                    {
                        if (l2wQuery[absorbers.Current].Position.y > l2w.Position.y)
                        {
                            availableLight -= lightQuery[absorbers.Current].SurfaceArea;
                        }
                    }

                    energyStore.Quantity -= absorber.SurfaceArea * math.pow(photosynthesis.Efficiency / 2, 2);
                    energyStore.Quantity += math.clamp(availableLight, 0, absorber.SurfaceArea) * photosynthesis.Efficiency;
                })
                .WithDisposeOnCompletion(lightCells)
                .WithName("Photosynthesis")
                .ScheduleParallel();
        }

        public static int2 GetCellIdFromPosition(float3 position, float cellSize)
        {
            return math.int2(new float2(math.floor(position.x / cellSize), math.floor(position.z / cellSize)));
        }

    }
}

