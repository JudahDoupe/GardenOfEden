using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct NodeRendererSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var sizeLookup = SystemAPI.GetComponentLookup<Size>(isReadOnly: true);
        var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(isReadOnly: true);
        new NodeRendererJob
        {
            SizeLookup = sizeLookup,
        }.Run();
        new CalculateInternodeRendererDataJob
        {
            SizeLookup = sizeLookup,
            LocalTransformLookup = localTransformLookup,
        }.Run();
        new UpdateInternodeRendererJob
        { }.Run();
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
    [ReadOnly]
    public ComponentLookup<LocalTransform> LocalTransformLookup;

    [BurstCompile]
    private void Execute(ref InternodeRenderer renderer)
    {
        var localPosition = LocalTransformLookup[renderer.Node].Position;
        var size = SizeLookup[renderer.Node];
        
        renderer.UniformScale = size.NodeRadius;
        renderer.LengthScale = size.InternodeLength / size.NodeRadius;
        renderer.LocalRotation = quaternion.LookRotationSafe(-localPosition, math.up());
    }
}

[BurstCompile]
public partial struct UpdateInternodeRendererJob : IJobEntity
{
    [BurstCompile]
    private void Execute(InternodeRenderer renderer, ref LocalTransform transform, ref PostTransformScale nonUniformScale)
    {
        transform.Scale = renderer.UniformScale;
        nonUniformScale.Value = new float3x3(1, 0, 0, 0, 1, 0, 0, 0, renderer.LengthScale);
        transform.Rotation = renderer.LocalRotation;
    }
}
