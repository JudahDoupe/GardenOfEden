using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Plants.Systems
{
    public struct DeterministicReproductionTrigger : IComponentData { }

    public class ReproductionTriggerSystem : SystemBase, IDailyProcess
    {
        public bool HasDayBeenProccessed() => true;
        public void ProcessDay()
        {
            Entities
                .WithNone<Dormant>()
                .WithAll<DeterministicReproductionTrigger>()
                .ForEach((ref NodeDivision nodeDivision) =>
                {
                    if (nodeDivision.RemainingDivisions >= 0) return;
                    nodeDivision.RemainingDivisions = 0;
                    nodeDivision.Type = NodeType.Reproduction;
                })
                .ScheduleParallel(Dependency)
                .Complete();
        }

        protected override void OnUpdate() { }
    }
}

