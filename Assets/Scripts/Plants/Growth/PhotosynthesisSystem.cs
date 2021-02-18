using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Setup;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Growth
{
    public struct Photosynthesis : IComponentData
    {
        public float Efficiency;
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    public class PhotosynthesisSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .ForEach((ref EnergyStore energyStore, in LightBlocker blocker, in LightAbsorber absorber, in LocalToWorld l2w, in Photosynthesis photosynthesis) =>
                {
                    energyStore.Quantity -= blocker.SurfaceArea * math.pow(photosynthesis.Efficiency / 2, 2);
                    energyStore.Quantity += math.clamp(absorber.AbsorbedLight, 0, blocker.SurfaceArea) * photosynthesis.Efficiency;
                })
                .WithName("Photosynthesis")
                .ScheduleParallel();
        }

    }
}

