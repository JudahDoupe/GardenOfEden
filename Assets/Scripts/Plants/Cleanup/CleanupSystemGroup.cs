using Assets.Scripts.Plants.Growth;
using Unity.Entities;

namespace Assets.Scripts.Plants.Cleanup
{
    [UpdateAfter(typeof(GrowthSystemGroup))]
    public class CleanupSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(CleanupSystemGroup), OrderLast = true)]
    public class CleanupEcbSystem : EntityCommandBufferSystem { }
}
