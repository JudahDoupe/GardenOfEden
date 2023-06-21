using Framework.Jobs;
using Framework.Utils;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct VelocityIntegrationSystem : ISystem
{
    public ComponentLookup<Parent> ParentLookup;
    public ComponentLookup<LocalTransform> LocalTransformLookup;
    public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
    public EntityQuery RecalculateLocalTransformQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        ParentLookup = state.GetComponentLookup<Parent>();
        LocalTransformLookup = state.GetComponentLookup<LocalTransform>();
        PostTransformMatrixLookup = state.GetComponentLookup<PostTransformMatrix>();
        RecalculateLocalTransformQuery = state.GetEntityQuery(typeof(LocalTransform), typeof(LocalToWorld));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.fixedDeltaTime;

        ParentLookup.Update(ref state);
        LocalTransformLookup.Update(ref state);
        PostTransformMatrixLookup.Update(ref state);

        state.Dependency = new IntegrateVelocityEuler
            {
                TimeStep = deltaTime
            }.ScheduleParallel(state.Dependency);

        state.Dependency = new RecalculateLocalToWorld
            {
                ParentLookup = ParentLookup,
                LocalTransformLookup = LocalTransformLookup,
                PostTransformMatrixLookup = PostTransformMatrixLookup
            }
            .ScheduleParallel(RecalculateLocalTransformQuery, state.Dependency);
    }
}

[BurstCompile]
public partial struct IntegrateVelocityEuler : IJobEntity
{
    public float TimeStep;

    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics,
                         RefRW<LocalTransform> localTransform,
                         LocalToWorld worldTransform)
    {
        localTransform.ValueRW.TranslateWorld(worldTransform, physics.ValueRO.Velocity * TimeStep);

        physics.ValueRW.Velocity += physics.ValueRO.Force / physics.ValueRO.Mass * TimeStep;
        physics.ValueRW.Force = 0;
    }
}