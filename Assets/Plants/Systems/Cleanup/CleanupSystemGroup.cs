using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Plants.Systems.Cleanup
{
    [UpdateAfter(typeof(GrowthSystemGroup))]
    public class CleanupSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(CleanupSystemGroup), OrderLast = true)]
    public class CleanupEcbSystem : EntityCommandBufferSystem { }
}
