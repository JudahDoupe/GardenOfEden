using Unity.Entities;
using Unity.Transforms;

public readonly partial struct GrowthAspect : IAspect
{
    public readonly Entity Entity;
    public readonly TransformAspect Transform;

    public readonly RefRO<PrimaryGrowth> PrimaryGrowthTarget;
    public readonly RefRW<Size> Size;

    public bool IsMature => Size.ValueRO.NodeRadius > 0.9f;
}
