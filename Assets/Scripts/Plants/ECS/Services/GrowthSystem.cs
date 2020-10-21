using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS.Services
{
    public struct PrimaryGrowth : IComponentData
    {
        public float GrowthRate;
        public float InternodeLength;
        public float InternodeRadius;
        public float3 NodeSize;
    }

    [UpdateAfter(typeof(EnergyFlowSystem))]
    public class GrowthSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                    (ref EnergyStore energyStore, ref Components.Node node, in PrimaryGrowth growth) =>
                    {
                        if (node.Size.Equals(growth.NodeSize))
                            return;

                        var desiredSize = math.min(node.Size + growth.GrowthRate, growth.NodeSize);

                        var currentVolume = math.PI * node.Size.x * node.Size.y * node.Size.z;
                        var desiredVolume = math.PI * desiredSize.x * desiredSize.y + desiredSize.z;
                        var desiredGrowth = desiredVolume - currentVolume;
                        var actuallyGrowth = math.min(energyStore.Quantity / 2, desiredGrowth);

                        node.Size = math.min(node.Size + actuallyGrowth, growth.NodeSize);
                        energyStore.Quantity -= actuallyGrowth;
                    })
                .WithName("GrowNode")
                .ScheduleParallel();

            Entities
                .ForEach(
                    (ref EnergyStore energyStore, ref Internode internode, ref Translation translation, in PrimaryGrowth growth) =>
                    {
                        if (internode.Length.Equals(growth.InternodeLength) && internode.Radius.Equals(growth.InternodeRadius))
                            return;

                        var desiredRadius = math.min(internode.Radius + growth.GrowthRate, growth.InternodeRadius);
                        var desiredLength = math.min(internode.Length + growth.GrowthRate, growth.InternodeLength);

                        var currentVolume = math.PI * math.pow(internode.Radius, 2) * internode.Length;
                        var desiredVolume = math.PI * math.pow(desiredRadius, 2) * desiredLength;
                        var desiredGrowth = desiredVolume - currentVolume;
                        var actuallyGrowth = math.min(energyStore.Quantity / 2, desiredGrowth);

                        internode.Radius = math.min(internode.Radius + actuallyGrowth, growth.InternodeRadius);
                        internode.Length = math.min(internode.Length + actuallyGrowth, growth.InternodeLength);
                        translation.Value.z = internode.Length;
                        energyStore.Quantity -= actuallyGrowth;
                    })
                .WithName("GrowInternode")
                .ScheduleParallel();
        }
    }
}
