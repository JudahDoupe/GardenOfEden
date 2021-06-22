using Assets.Scripts.Plants.Growth;
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
            var planet = Planet.Entity;
            var lightCells = new NativeMultiHashMap<int3, Entity>(Coordinate.TextureWidthInPixels * Coordinate.TextureWidthInPixels * 6, Allocator.TempJob);
            var lightCellsWriter = lightCells.AsParallelWriter();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach((ref LightBlocker blocker, in Entity entity, in LocalToWorld l2w) =>
                {
                    var nodeQuery = GetComponentDataFromEntity<Node>(true);

                    if (nodeQuery.HasComponent(entity))
                    {
                        var node = nodeQuery[entity]; 
                        var internodeSize = new float3(2 * node.InternodeRadius, 2 * node.InternodeRadius, node.InternodeLength);
                        blocker.SurfaceArea = GetSurfaceArea(l2w, node.Size) + GetSurfaceArea(l2w, internodeSize);
                    }

                    blocker.CellId = new Coordinate(l2w.Position, GetComponent<LocalToWorld>(planet)).TextureXyw;
                    lightCellsWriter.Add(blocker.CellId, entity);

                })
                .WithoutBurst()
                .WithName("UpdateLightBlockers")
                .Run();

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
            if (l2w.Position.Equals(float3.zero))
            {
                return 0;
            }
            var globalUp = math.normalize(l2w.Position);
            var x = GetFaceRatio(l2w.Right, globalUp) * size.z * size.y;
            var y = GetFaceRatio(l2w.Up, globalUp) * size.z * size.x;
            var z = GetFaceRatio(l2w.Forward, globalUp) * size.x * size.y;
            var sa = x + y + z;
            return sa;
        }

        private static float GetFaceRatio(float3 faceDir, float3 globalUp)
        {
            var dot = math.clamp(math.dot(faceDir, globalUp), -1, 1);
            var arcCos = math.acos(dot);
            var angle = math.degrees(arcCos);
            var t = math.abs(angle - 90) / 90;
            return t;
        }

    }
}

