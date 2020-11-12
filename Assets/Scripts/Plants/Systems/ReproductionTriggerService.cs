using System;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Plants.Systems
{
    public struct DeterministicReproductionTrigger : IComponentData { }

    public class ReproductionTriggerSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .WithAll<DeterministicReproductionTrigger>()
                .ForEach((ref NodeDivision nodeDivision) =>
                {
                    if (nodeDivision.RemainingDivisions >= 0) return;
                    nodeDivision.RemainingDivisions = 0;
                    nodeDivision.Type = NodeType.Reproduction;
                })
                .ScheduleParallel();
        }
    }
}

