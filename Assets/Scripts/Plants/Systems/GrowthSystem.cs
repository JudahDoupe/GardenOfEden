using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    public struct Node : IComponentData
    {
        public float3 Size;
        public float Volume => Size.x * Size.y * Size.z * 1.333f * math.PI;
    }

    public struct Internode : IComponentData
    {
        public float Length;
        public float Radius;
        public float Volume => Length * Radius * Radius * math.PI;
    }

    public struct PrimaryGrowth : IComponentData
    {
        public float GrowthRate;
        public float InternodeLength;
        public float InternodeRadius;
        public float3 NodeSize;
    }

    public class GrowthSystem : SystemBase, IDailyProcess
    {
        public void ProcessDay(Action callback)
        {
            Entities
                .WithNone<Dormant>()
                .ForEach(
                    (ref EnergyStore energyStore, ref Node node, in PrimaryGrowth growth) =>
                    {
                        if (node.Size.Equals(growth.NodeSize))
                            return;

                        var desiredNode = new Node
                        {
                            Size = math.min(node.Size + growth.GrowthRate, growth.NodeSize)
                        };
                        var desiredVolumeGrowth = desiredNode.Volume - node.Volume;
                        var constrainedVolumeGrowth = math.min(energyStore.Quantity * 2, desiredVolumeGrowth);
                        var constrainedGrowthRate = math.pow(constrainedVolumeGrowth / (1.333f * math.PI), 1f / 3f);

                        node.Size = math.min(node.Size + constrainedGrowthRate, growth.NodeSize);
                        energyStore.Quantity -= constrainedVolumeGrowth / 4;
                    })
                .WithName("GrowNode")
                .ScheduleParallel(Dependency)
                .Complete();

            Entities
                .WithNone<Dormant>()
                .ForEach(
                    (ref EnergyStore energyStore, ref Internode internode, ref Translation translation, in PrimaryGrowth growth) =>
                    {
                        if (internode.Length.Equals(growth.InternodeLength) && internode.Radius.Equals(growth.InternodeRadius))
                            return;

                        var desiredInternode = new Internode
                        {
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
                .ScheduleParallel(Dependency)
                .Complete();

            callback();
        }

        protected override void OnUpdate() { }
    }
}
