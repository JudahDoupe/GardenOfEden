using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(PlantSimulationGroup))]
[UpdateAfter(typeof(ReplicationSystem))]
public partial struct NodeRendererSystem : ISystem
{
    private ComponentLookup<Size> _sizeLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _sizeLookup = SystemAPI.GetComponentLookup<Size>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _sizeLookup.Update(ref state);

        state.Dependency = new NodeRendererJob
        {
            SizeLookup = _sizeLookup
        }.ScheduleParallel(state.Dependency);
        state.Dependency = new InternodeRendererJob
        {
            SizeLookup = _sizeLookup
        }.ScheduleParallel(state.Dependency);
    }
}

public partial struct NodeRendererJob : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<Size> SizeLookup;

    [BurstCompile]
    private void Execute(NodeRenderer renderer, 
                         ref LocalTransform transform)
    {
        transform.Scale = SizeLookup[renderer.Node].NodeRadius;
    }
}

public partial struct InternodeRendererJob : IJobEntity
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