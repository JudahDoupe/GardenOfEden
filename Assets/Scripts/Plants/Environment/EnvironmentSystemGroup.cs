using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using Unity.Entities;

namespace Assets.Scripts.Plants.Environment
{
    [UpdateAfter(typeof(SetupSystemGroup))]
    public class EnvironmentSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(EnvironmentSystemGroup), OrderLast = true)]
    public class EnvironmentEcbSystem : EntityCommandBufferSystem { }
}
