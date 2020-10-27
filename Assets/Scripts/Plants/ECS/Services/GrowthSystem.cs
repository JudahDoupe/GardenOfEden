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
                .WithNone<Dormant>()
                .ForEach(
                    (ref EnergyStore energyStore, ref Components.Node node, in PrimaryGrowth growth) =>
                    {
                        if (node.Size.Equals(growth.NodeSize))
                            return;

                        var desiredNode = new Components.Node { 
                            Size = math.min(node.Size + growth.GrowthRate, growth.NodeSize) 
                        };
                        var desiredVolumeGrowth = desiredNode.Volume - node.Volume;
                        var constrainedVolumeGrowth = math.min(energyStore.Quantity * 2, desiredVolumeGrowth);
                        var constrainedGrowthRate = math.pow(constrainedVolumeGrowth / (1.333f * math.PI), 1f/3f);

                        node.Size = math.min(node.Size + constrainedGrowthRate, growth.NodeSize);
                        energyStore.Quantity -= constrainedVolumeGrowth / 4;
                    })
                .WithName("GrowNode")
                .ScheduleParallel();

            Entities
                .WithNone<Dormant>()
                .ForEach(
                    (ref EnergyStore energyStore, ref Internode internode, ref Translation translation, in PrimaryGrowth growth) =>
                    {
                        if (internode.Length.Equals(growth.InternodeLength) && internode.Radius.Equals(growth.InternodeRadius))
                            return;

                        var desiredInternode = new Internode {
                            Radius = math.min(internode.Radius + growth.GrowthRate, growth.InternodeRadius),
                            Length = math.min(internode.Length + growth.GrowthRate, growth.InternodeLength), 
                        };
                        var desiredVolumeGrowth = desiredInternode.Volume - internode.Volume;
                        var constrainedVolumeGrowth = math.min(energyStore.Quantity * 2, desiredVolumeGrowth);
                        var constrainedGrowthRate = math.pow(constrainedVolumeGrowth / math.PI, 1f / 3f);

                        internode.Radius = math.min(internode.Radius + constrainedGrowthRate, growth.InternodeRadius);
                        internode.Length = math.min(internode.Length + constrainedGrowthRate, growth.InternodeLength);
                        translation.Value.z = internode.Length;
                        energyStore.Quantity -= constrainedVolumeGrowth / 4;
                    })
                .WithName("GrowInternode")
                .ScheduleParallel();
        }
    }
}
