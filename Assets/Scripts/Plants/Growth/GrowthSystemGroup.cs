using Unity.Entities;

namespace Assets.Scripts.Plants.Growth
{
    public class GrowthSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GrowthSystemGroup), OrderLast = true)]
    public class GrowthEcbSystem : EntityCommandBufferSystem { }
}
