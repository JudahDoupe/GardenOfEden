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
        public static readonly float LightLevel = 1;
        public static float PlanetArea => 4 * math.PI * math.pow(Coordinate.PlanetRadius, 2);
        public static int NumCells => Coordinate.TextureWidthInPixels * Coordinate.TextureWidthInPixels * 6;
        public static float CellArea => PlanetArea / NumCells;
        public static float LightPerCell => CellArea * LightLevel;

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

                    var hasInternode = internodeQuery.HasComponent(entity);
                    var hasNode = nodeQuery.HasComponent(entity);

                    if (hasInternode || hasNode)
                    {
                        blocker.SurfaceArea = 0;
                    }
                    if (hasInternode)
                    {
                        var internode = internodeQuery[entity];
                        var size = new float3(2 * internode.Radius, 2 * internode.Radius, internode.Length);
                        blocker.SurfaceArea += GetSurfaceArea(l2w, size);
                    }
                    if (hasNode)
                    {
                        var node = nodeQuery[entity];
                        blocker.SurfaceArea += GetSurfaceArea(l2w, node.Size);
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

                    var availableLight = LightPerCell;

                    var absorbers = lightCells.GetValuesForKey(blocker.CellId);
                    while (availableLight > 0 && absorbers.MoveNext())
                    {
                        if (l2wQuery[absorbers.Current].Position.y > l2w.Position.y)
                        {
                            availableLight -= lightQuery[absorbers.Current].SurfaceArea;
                            availableLight = math.max(availableLight, 0);
                        }
                    }

                    absorber.AbsorbedLight = math.min(availableLight, blocker.SurfaceArea);
                })
                .WithDisposeOnCompletion(lightCells)
                .WithName("UpdateAbsorbedLight")
                .ScheduleParallel();
        }

        private static float GetSurfaceArea(LocalToWorld l2w, float3 size)
        {
            var globalUp = math.normalize(l2w.Position);
            var x = GetFaceRatio(l2w.Right, globalUp) * size.z * size.y;
            var y = GetFaceRatio(l2w.Up, globalUp) * size.z * size.x;
            var z = GetFaceRatio(l2w.Forward, globalUp) * size.x * size.y;
            return x + y + z;
        }

        private static float GetFaceRatio(float3 faceDir, float3 globalUp)
        {
            var angle = math.degrees(math.acos(math.dot(faceDir, globalUp)));
            var t = math.abs(angle - 90) / 90; 
            return t;
        }

    }
}

