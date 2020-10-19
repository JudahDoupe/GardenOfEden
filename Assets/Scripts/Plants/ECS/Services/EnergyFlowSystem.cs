using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Assets.Scripts.Plants.ECS.Services.TransportationSystems
{
    public struct EnergyStore : IComponentData
    {
        public float Quantity;
        public float Capacity;
    }

    public struct EnergyFlow : IComponentData
    {
        public float Throughput;
    }

    public class EnergyFlowSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                    (ref EnergyFlow energyFlow, in Internode internode) =>
                    {
                        var energyStoreQuery = GetComponentDataFromEntity<EnergyStore>(true);
                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        var headStore = energyStoreQuery[internode.HeadNode];
                        var tailStore = energyStoreQuery[internode.TailNode];
                        var numBranches = childrenQuery.HasComponent(internode.TailNode) ? childrenQuery[internode.TailNode].Length + 1 : 1;

                        var resistance = 0f;
                        var flowRate = (1f / numBranches) / (1 + resistance);
                        var headPressure = headStore.Quantity / (headStore.Capacity + float.Epsilon);
                        var tailPressure = tailStore.Quantity / (tailStore.Capacity + float.Epsilon);

                        if (tailPressure > headPressure)
                        {
                            energyFlow.Throughput = flowRate * tailStore.Quantity * (tailPressure - headPressure);
                        }
                        else
                        {
                            energyFlow.Throughput = -flowRate * headStore.Quantity * (headPressure - tailPressure);
                        }
                    })
                .WithName("UpdateEnergyThroughput")
                .ScheduleParallel();

            Entities
                .ForEach(
                    (ref EnergyStore energyStore, in InternodeReference internodeRef, in Entity entity) =>
                    {
                        var internodeRefQuery = GetComponentDataFromEntity<InternodeReference>(true);
                        var internodeQuery = GetComponentDataFromEntity<Internode>(true);
                        var energyFlowQuery = GetComponentDataFromEntity<EnergyFlow>(true);

                        var boundsQuery = GetComponentDataFromEntity<RenderBounds>(true);
                        var scaleQuery = GetComponentDataFromEntity<Scale>(true);
                        var nonUniformScaleQuery = GetComponentDataFromEntity<NonUniformScale>(true);

                        var childrenQuery = GetBufferFromEntity<Child>(true);

                        var extents = new float3(0.001f, 0.001f, 0.001f);
                        if (boundsQuery.HasComponent(entity))
                        {
                            if (scaleQuery.HasComponent(entity))
                            {
                                extents = boundsQuery[entity].Value.Extents * scaleQuery[entity].Value;
                            }
                            else if (nonUniformScaleQuery.HasComponent(entity))
                            {
                                extents = boundsQuery[entity].Value.Extents * nonUniformScaleQuery[entity].Value;
                            }
                            else
                            {
                                extents = boundsQuery[entity].Value.Extents;
                            }
                        }
                        var nodeCapacity = GetNodeCapacity(extents);
                        var internodeCapacity = GetInternodeCapacity(internodeQuery[internodeRefQuery[entity].Internode]);
                        energyStore.Capacity = internodeCapacity + nodeCapacity;


                        if (energyFlowQuery.HasComponent(internodeRef.Internode))
                        {
                            energyStore.Quantity += energyFlowQuery[internodeRef.Internode].Throughput;
                        }

                        if (childrenQuery.HasComponent(entity))
                        {
                            var branches = childrenQuery[entity];

                            for (int i = 0; i < branches.Length; i++)
                            {
                                var internodeEntity = internodeRefQuery[branches[i].Value].Internode;
                                energyStore.Quantity -= energyFlowQuery[internodeEntity].Throughput;
                            }
                        }

                        energyStore.Quantity = math.clamp(energyStore.Quantity, 0, energyStore.Capacity);
                    })
                .WithName("UpdateEnergyQuantities")
                .ScheduleParallel();
        }

        private static float GetInternodeCapacity(Internode internode)
        {
            return internode.Length * internode.Radius * internode.Radius * math.PI;
        }

        private static float GetNodeCapacity(float3 extents)
        {
            return extents.x * extents.y * extents.z * 1.333f * math.PI;
        }
    }
}
