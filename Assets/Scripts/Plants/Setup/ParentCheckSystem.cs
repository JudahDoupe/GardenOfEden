using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Setup
{

    [UpdateInGroup(typeof(SetupSystemGroup))]
    public class ParentCheckSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<Dormant>()
                .ForEach(
                    (in Parent parent, in Entity entity) =>
                    {
                        if (parent.Value == Entity.Null)
                        {
                            Debug.Log($"{entity.Index}");
                        }
                    })
                .WithName("FindUnparentedChildren")
                .ScheduleParallel();

        }
    }
}
