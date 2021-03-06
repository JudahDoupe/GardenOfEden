﻿using Assets.Scripts.Plants.Cleanup;
using Unity.Entities;

namespace Assets.Scripts.Plants.Growth
{
    public struct DeterministicReproductionTrigger : IComponentData { }
    public struct AnnualReproductionTrigger : IComponentData
    {
        public int Month;
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateBefore(typeof(NodeDivisionSystem))]
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
                    if (nodeDivision.RemainingDivisions < 0)
                    {
                        StartReproduction(ref nodeDivision);
                    }
                })
                .ScheduleParallel();

            var monthOfTheYear = Singleton.TimeService.MonthOfTheYear;
            var dayOfTheMonth = Singleton.TimeService.DayOfTheMonth;
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref NodeDivision nodeDivision, in AnnualReproductionTrigger trigger) =>
                {
                    if (monthOfTheYear == trigger.Month && dayOfTheMonth == 0)
                    {
                        StartReproduction(ref nodeDivision);
                    }
                })
                .ScheduleParallel();
        }

        private static void StartReproduction(ref NodeDivision nodeDivision)
        {
            nodeDivision.RemainingDivisions = 0;
            nodeDivision.Type = NodeType.Reproduction;
        }
    }
}

