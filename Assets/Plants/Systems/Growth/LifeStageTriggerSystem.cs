using System.Linq;
using Assets.Scripts.Plants.Cleanup;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Growth
{
    public struct DeterministicLifeStageTrigger : IComponentData
    {
        public LifeStage CurrentStage;
        public LifeStage NextStage;
        public int NextStageDivisions;
    }
    public struct AnnualLifeStageTrigger : IComponentData
    {
        public int Month;
        public LifeStage Stage;
    }
    public struct TimeDelayedLifeStageTrigger : IComponentData
    {
        public int Days;
        public LifeStage Stage;
    }
    public struct ParentLifeStageTrigger : IComponentData
    {
        public LifeStage ParentedStage;
        public LifeStage UnparentedStage;
    }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateBefore(typeof(NodeDivisionSystem))]
    public class LifeStageTriggerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref NodeDivision nodeDivision, in DeterministicLifeStageTrigger trigger) =>
                {
                    if (nodeDivision.RemainingDivisions < 0 && nodeDivision.Stage == trigger.CurrentStage)
                    {
                        nodeDivision.Stage = trigger.NextStage;
                        nodeDivision.RemainingDivisions = trigger.NextStageDivisions; 
                    }
                })
                .ScheduleParallel();

            var monthOfTheYear = Singleton.TimeService.MonthOfTheYear;
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref NodeDivision nodeDivision, in AnnualLifeStageTrigger trigger) =>
                {
                    if (monthOfTheYear == trigger.Month)
                    {
                        nodeDivision.Stage = trigger.Stage;
                    }
                })
                .ScheduleParallel();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref NodeDivision nodeDivision, ref TimeDelayedLifeStageTrigger trigger) =>
                {
                    trigger.Days--;
                    if (trigger.Days == 0)
                    {
                        nodeDivision.Stage = trigger.Stage;
                    }
                })
                .ScheduleParallel();

            var planet = Planet.Entity;
            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithNone<Dormant>()
                .ForEach((ref NodeDivision nodeDivision, in Parent parent, in ParentLifeStageTrigger trigger, in Entity e) =>
                {
                    nodeDivision.Stage = parent.Value != planet ? trigger.ParentedStage : trigger.UnparentedStage;
                })
                .ScheduleParallel();
        }

    }
}

