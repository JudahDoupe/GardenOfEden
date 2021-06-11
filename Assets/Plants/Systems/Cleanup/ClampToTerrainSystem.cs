using System;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Cleanup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(CleanupSystemGroup))]
    [UpdateBefore(typeof(DeadNodeSystem))]
    public class ClampToTerrainSystem : SystemBase
    {
        CleanupEcbSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<CleanupEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var seaLevel = LandService.SeaLevel;
            var landMaps = EnvironmentDataStore.LandMap.CachedTextures().Select(x => x.GetRawTextureData<Color>()).ToArray();
            var landMaps0 = landMaps[0];
            var landMaps1 = landMaps[1];
            var landMaps2 = landMaps[2];
            var landMaps3 = landMaps[3];
            var landMaps4 = landMaps[4];
            var landMaps5 = landMaps[5];
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNativeDisableParallelForRestriction(landMaps0)
                .WithNativeDisableParallelForRestriction(landMaps1)
                .WithNativeDisableParallelForRestriction(landMaps2)
                .WithNativeDisableParallelForRestriction(landMaps3)
                .WithNativeDisableParallelForRestriction(landMaps4)
                .WithNativeDisableParallelForRestriction(landMaps5)
                .ForEach( 
                    (ref Coordinate coord, ref Translation translation, in Entity entity) =>
                    {
                        var landMap = coord.w switch
                        {
                            0 => landMaps0,
                            1 => landMaps1,
                            2 => landMaps2,
                            3 => landMaps3,
                            4 => landMaps4,
                            5 => landMaps5,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        coord.Altitude = seaLevel + landMap[coord.nativeArrayIndex].r;
                        translation.Value = coord.xyz;
                    })
                .WithName("ClampToTerrain")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}