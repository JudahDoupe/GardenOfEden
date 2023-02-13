using Unity.Entities;
using Unity.Transforms;

public readonly partial struct GrowthAspect : IAspect
{
    public readonly Entity Entity;
    public readonly TransformAspect Transform;

    public readonly RefRO<PrimaryGrowthTarget> PrimaryGrowthTarget;
    public readonly RefRW<Size> Size;

    public bool IsMature => Size.ValueRO.NodeSize > 0.9f;
}
