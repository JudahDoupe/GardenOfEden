using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Environment
{
    public struct LightBlocker : IComponentData
    {
        public float SurfaceArea;
        public int3 CellId;
    }
    public struct LightAbsorber : IComponentData
    {
        public float AbsorbedLight;
    }

    [UpdateInGroup(typeof(EnvironmentSystemGroup))]
    public class LightSystem : SystemBase
    {
        public const float LightPerCell = 0.01f;

        protected override void OnUpdate()
        {
            var lightCells = new NativeMultiHashMap<int3, Entity>(Coordinate.TextureWidthInPixels * Coordinate.TextureWidthInPixels * 6, Allocator.TempJob);
            var lightCellsWriter = lightCells.AsParallelWriter();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach((ref LightBlocker blocker, in Entity entity, in LocalToWorld l2w) =>
                {
                    var internodeQuery = GetComponentDataFromEntity<Internode>(true);
                    var nodeQuery = GetComponentDataFromEntity<Node>(true);

                    //TODO: rotation should be checked against world up

                    blocker.SurfaceArea = 0;
                    if (internodeQuery.HasComponent(entity))
                    {
                        var internode = internodeQuery[entity];
                        var globalSize = math.mul(l2w.Rotation,
                            new float3(internode.Radius, internode.Radius, internode.Length));
                        blocker.SurfaceArea += math.abs(globalSize.x * globalSize.z);
                    }

                    if (nodeQuery.HasComponent(entity))
                    {
                        var node = nodeQuery[entity];
                        var globalSize = math.mul(l2w.Rotation, node.Size);
                        blocker.SurfaceArea += math.abs(globalSize.x * globalSize.z);
                    }

                    blocker.CellId = new Coordinate(l2w.Position).xyw;
                    lightCellsWriter.Add(blocker.CellId, entity);

                })
                .WithName("UpdateLightBlockers")
                .ScheduleParallel();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNativeDisableParallelForRestriction(lightCells)
                .ForEach((ref LightAbsorber absorber, in LightBlocker blocker, in LocalToWorld l2w) =>
                {
                    var l2wQuery = GetComponentDataFromEntity<LocalToWorld>(true);
                    var lightQuery = GetComponentDataFromEntity<LightBlocker>(true);

                    var availableLight = math.pow(Coordinate.PlanetRadius, 3) / (math.pow(Coordinate.TextureWidthInPixels, 2) * 6) * LightPerCell;

                    var absorbers = lightCells.GetValuesForKey(blocker.CellId);
                    while (absorbers.MoveNext())
                    {
                        if (l2wQuery[absorbers.Current].Position.y > l2w.Position.y)
                        {
                            availableLight -= lightQuery[absorbers.Current].SurfaceArea;
                        }
                    }

                    absorber.AbsorbedLight = availableLight;
                })
                .WithDisposeOnCompletion(lightCells)
                .WithName("UpdateAbsorbedLight")
                .ScheduleParallel();
        }

    }
}

