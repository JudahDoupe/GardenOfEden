using Assets.Scripts.Plants.Environment;
using Unity.Entities;

namespace Assets.Scripts.Plants.Growth
{
    [UpdateAfter(typeof(EnvironmentSystemGroup))]
    public class GrowthSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GrowthSystemGroup), OrderLast = true)]
    public class GrowthEcbSystem : EntityCommandBufferSystem { }
}
