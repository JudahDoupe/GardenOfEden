using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Setup
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class SetupSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(SetupSystemGroup), OrderLast = true)]
    public class SetupEcbSystem : EntityCommandBufferSystem { }
}
