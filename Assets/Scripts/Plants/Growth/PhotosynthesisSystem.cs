﻿using Assets.Scripts.Plants.Environment;
using Unity.Entities;

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
                .ForEach((ref EnergyStore energyStore, in LightAbsorber absorber, in Photosynthesis photosynthesis) =>
                {
                    energyStore.Quantity += absorber.AbsorbedLight * photosynthesis.Efficiency;
                })
                .WithName("Photosynthesis")
                .ScheduleParallel();
        }

    }
}

