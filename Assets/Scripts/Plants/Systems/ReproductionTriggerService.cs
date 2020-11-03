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
                .WithNone<Dormant>()
                .WithAll<DeterministicReproductionTrigger>()
                .ForEach((ref NodeDivision nodeDivision) =>
                {
                    if (nodeDivision.RemainingDivisions >= 0) return;
                    nodeDivision.RemainingDivisions = 0;
                    nodeDivision.Type = EmbryoNodeType.Reproduction;
                })
                .ScheduleParallel();
        }
    }
}

