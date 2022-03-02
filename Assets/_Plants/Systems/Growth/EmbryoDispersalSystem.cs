using System;
using System.Linq;
using Assets.Plants.Systems.Cleanup;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Assets.Scripts.Plants.Growth
{
    public struct WindDispersal : IComponentData { }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateAfter(typeof(GrowthSystem))]
    public class EmbryoDispersalSystem : SystemBase
    {
        GrowthEcbSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<GrowthEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var genericSeed = new System.Random().Next();
            var planet = Planet.Entity;
            var landMaps = EnvironmentMapDataStore.LandHeightMap.CachedTextures().Select(x => x.GetRawTextureData<float>()).ToArray();
            var landMaps0 = landMaps[0];
            var landMaps1 = landMaps[1];
            var landMaps2 = landMaps[2];
            var landMaps3 = landMaps[3];
            var landMaps4 = landMaps[4];
            var landMaps5 = landMaps[5];

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithAll<WindDispersal, Parent, LocalToParent>()
                .WithAll<Translation, Rotation>()
                .WithNativeDisableParallelForRestriction(landMaps0)
                .WithNativeDisableParallelForRestriction(landMaps1)
                .WithNativeDisableParallelForRestriction(landMaps2)
                .WithNativeDisableParallelForRestriction(landMaps3)
                .WithNativeDisableParallelForRestriction(landMaps4)
                .WithNativeDisableParallelForRestriction(landMaps5)
                .ForEach((in Node node, in PrimaryGrowth growth, in EnergyStore energy, in Parent parent, in LocalToWorld l2w, in Entity entity, in int entityInQueryIndex) =>
                {
                    if (parent.Value == planet || node.Volume < growth.Volume || energy.Pressure < 0.9f) return;

                    var seed = math.asuint((genericSeed * entityInQueryIndex) % uint.MaxValue) + 1;
                    var rand = new Random(seed);
                    var pl2w = GetComponent<LocalToWorld>(planet);

                    var coord = new Coordinate(l2w.Position, pl2w);
                    var landMap = coord.TextureW switch
                    {
                        0 => landMaps0,
                        1 => landMaps1,
                        2 => landMaps2,
                        3 => landMaps3,
                        4 => landMaps4,
                        5 => landMaps5,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    var height = landMap[coord.NativeArrayId];
                    var distance = math.max(10, (coord.Altitude - height) * 10);
                    
                    coord.Lon += rand.NextFloat(-distance, distance);
                    coord.Lat += rand.NextFloat(-distance, distance);
                    landMap = coord.TextureW switch
                    {
                        0 => landMaps0,
                        1 => landMaps1,
                        2 => landMaps2,
                        3 => landMaps3,
                        4 => landMaps4,
                        5 => landMaps5,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    coord.Altitude = landMap[coord.NativeArrayId];

                    ecb.RemoveComponent<WindDispersal>(entityInQueryIndex, entity);
                    ecb.AddComponent<Coordinate>(entityInQueryIndex, entity);
                    ecb.SetComponent(entityInQueryIndex, entity, coord);
                    ecb.SetComponent(entityInQueryIndex, entity, new Parent {Value = planet});
                    ecb.SetComponent(entityInQueryIndex, entity, new Translation { Value = coord.LocalPlanet });
                    ecb.SetComponent(entityInQueryIndex, entity, new Rotation { Value = quaternion.LookRotation(math.normalize(coord.LocalPlanet), new float3(0,1,0)) });
                })
                .WithName("WindEmbryoDispersal")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}

