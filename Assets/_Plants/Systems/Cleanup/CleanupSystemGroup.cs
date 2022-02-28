using Assets.Scripts.Plants.Growth;
using Unity.Entities;

namespace Assets.Plants.Systems.Cleanup
{
    [UpdateAfter(typeof(GrowthSystemGroup))]
    public class CleanupSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(CleanupSystemGroup))]
    [UpdateBefore(typeof(DeleteEntityEcbSystem))]
    public class CleanupEcbSystem : EntityCommandBufferSystem { }

    [UpdateInGroup(typeof(CleanupSystemGroup), OrderLast = true)]
    public class DeleteEntityEcbSystem : EntityCommandBufferSystem { }
}
