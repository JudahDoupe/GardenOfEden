using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PlantSimulationGroup))]
public partial struct NodeRendererSystem : ISystem
{
    private ComponentLookup<Size> _sizeLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _sizeLookup = SystemAPI.GetComponentLookup<Size>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _sizeLookup.Update(ref state);

        new NodeRendererJob
        {
            SizeLookup = _sizeLookup
        }.Run();
        new CalculateInternodeRendererDataJob
        {
            SizeLookup = _sizeLookup
        }.Run();
    }
}

[BurstCompile]
public partial struct NodeRendererJob : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<Size> SizeLookup;

    [BurstCompile]
    private void Execute(NodeRenderer renderer, TransformAspect transformAspect)
    {
        transformAspect.WorldScale = SizeLookup[renderer.Node].NodeRadius;
    }
}

[BurstCompile]
public partial struct CalculateInternodeRendererDataJob : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<Size> SizeLookup;

    [BurstCompile]
    private void Execute(InternodeRenderer renderer,
                         ref LocalTransform transform,
                         ref PostTransformScale nonUniformScale)
    {
        var size = SizeLookup[renderer.Node];

        transform.Scale = size.NodeRadius;
        nonUniformScale.Value = new float3x3(1, 0, 0, 0, 1, 0, 0, 0, size.InternodeLength / size.NodeRadius);
    }
}