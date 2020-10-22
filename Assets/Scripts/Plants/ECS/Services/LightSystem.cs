using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using Assets.Scripts.Plants.ECS.Components;

namespace Assets.Scripts.Plants.ECS.Services
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

    public class LightSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float cellSize = 5;

            var query = GetEntityQuery(typeof(LightAbsorption), typeof(LocalToWorld));
            var lightCells = new NativeMultiHashMap<int2, Entity>(query.CalculateEntityCount(), Allocator.TempJob);
            var lightCellsWriter = lightCells.AsParallelWriter();

            var job = Entities
                .ForEach((ref LightAbsorption absorber, in Entity entity, in LocalToWorld l2w) =>
                {
                    var internodeQuery = GetComponentDataFromEntity<Internode>(true);
                    var nodeQuery = GetComponentDataFromEntity<Components.Node>(true);

                    absorber.SurfaceArea = 0;
                    if (internodeQuery.HasComponent(entity))
                    {
                        var internode = internodeQuery[entity];
                        var globalSize = math.mul(l2w.Rotation, new float3(internode.Radius, internode.Radius, internode.Length));
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
                .ScheduleParallel(Dependency);

            job.Complete();

            Entities
                .WithNativeDisableParallelForRestriction(lightCells)
                .ForEach((ref EnergyStore energyStore, in LightAbsorption absorber, in LocalToWorld l2w, in Photosynthesis photosynthesis) =>
                {
                    var l2wQuery = GetComponentDataFromEntity<LocalToWorld>(true);
                    var lightQuery = GetComponentDataFromEntity<LightAbsorption>(true);

                    var availableLight = cellSize * cellSize;
                    if (lightCells.TryGetFirstValue(absorber.CellId, out var shadingEntity, out var iterator))
                    {
                        do
                        {
                            if (l2wQuery[shadingEntity].Position.y > l2w.Position.y)
                            {
                                availableLight -= lightQuery[shadingEntity].SurfaceArea;
                            }
                        } while (availableLight > 0 && lightCells.TryGetNextValue(out shadingEntity, ref iterator));
                    }

                    energyStore.Quantity += math.clamp(availableLight, absorber.SurfaceArea, 0) * photosynthesis.Efficiency;
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

