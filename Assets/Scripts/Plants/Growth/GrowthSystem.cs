﻿using Assets.Scripts.Plants.Cleanup;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Growth
{
    public struct Node : IComponentData
    {
        public float3 Size;
        public float InternodeLength;
        public float InternodeRadius;
        public float Volume => (Size.x * Size.y * Size.z) + (InternodeLength * InternodeRadius * InternodeRadius * math.PI);
    }

    public struct PrimaryGrowth : IComponentData
    {
        public int DaysToMature;
        public float InternodeLength;
        public float InternodeRadius;
        public float3 NodeSize;
        public float Volume => (NodeSize.x * NodeSize.y * NodeSize.z) + (InternodeLength * InternodeRadius * InternodeRadius * math.PI);
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateAfter(typeof(MetabolismSystem))]
    public class GrowthSystem : SystemBase
    {
        public const float EnergyToVolumeRatio = 0.25f; 

        protected override void OnUpdate()
        {
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach(
                    (ref EnergyStore energyStore, ref Node node, ref Translation translation, in PrimaryGrowth growth) =>
                    {
                        var currentVolume = node.Volume;
                        var maxVolume = growth.Volume;
                        var remainingVolume = maxVolume - currentVolume;

                        if (remainingVolume <= 0)
                            return;

                        var desiredGrowth = math.min(maxVolume / growth.DaysToMature, remainingVolume);
                        if (remainingVolume - desiredGrowth < desiredGrowth / 1000)
                        {
                            desiredGrowth = remainingVolume;
                        }

                        var availableEnergy = energyStore.Quantity / 2;
                        var constrainedGrowth = math.clamp(desiredGrowth, 0, availableEnergy / EnergyToVolumeRatio);
                        var usedEnergy = constrainedGrowth * EnergyToVolumeRatio;

                        var newVolume = currentVolume + constrainedGrowth;

                        var t = math.pow(newVolume / maxVolume, 1 / 3f);

                        node.Size = growth.NodeSize * t;
                        node.InternodeRadius = growth.InternodeRadius * t;
                        node.InternodeLength = growth.InternodeLength * t;
                        translation.Value.z = node.InternodeLength;
                        energyStore.Quantity -= usedEnergy;
                    })
                .WithName("PrimaryGrowth")
                .ScheduleParallel();
        }
    }
}
