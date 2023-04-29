using Framework.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

[UpdateAfter(typeof(CollisionSystem))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct ConstraintSystem : ISystem
{
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    public ComponentLookup<LocalToWorld> WorldTransformLookup;
    public ComponentLookup<Parent> ParentLookup;
    public ComponentLookup<LocalTransform> LocalTransformLookup;
    public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
    public EntityQuery RecalculateLocalTransformQuery;

    private bool _haveTransformsInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        PhysicsLookup = state.GetComponentLookup<PhysicsBody>();
        WorldTransformLookup = state.GetComponentLookup<LocalToWorld>();
        ParentLookup = state.GetComponentLookup<Parent>();
        LocalTransformLookup = state.GetComponentLookup<LocalTransform>();
        PostTransformMatrixLookup = state.GetComponentLookup<PostTransformMatrix>();
        RecalculateLocalTransformQuery = state.GetEntityQuery(typeof(LocalTransform), typeof(LocalToWorld));
        _haveTransformsInitialized = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!_haveTransformsInitialized)
        {
            _haveTransformsInitialized = true;
            return;
        }

        PhysicsLookup.Update(ref state);
        WorldTransformLookup.Update(ref state);
        ParentLookup.Update(ref state);
        LocalTransformLookup.Update(ref state);
        PostTransformMatrixLookup.Update(ref state);

        state.Dependency = new SolveLengthConstraint
            {
                PhysicsLookup = PhysicsLookup,
                WorldTransformLookup = WorldTransformLookup
            }
            .ScheduleParallel(state.Dependency);

        state.Dependency = new ResolveFaceDirectionConstraint()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new ResolveConstraints()
            .ScheduleParallel(state.Dependency);

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
public partial struct SolveLengthConstraint : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    [ReadOnly]
    public ComponentLookup<LocalToWorld> WorldTransformLookup;

    [BurstCompile]
    private void Execute(Entity e,
                         Parent parent,
                         LengthConstraint length,
                         RefRW<ConstraintResponse> constraint)
    {
        var myPosition = WorldTransformLookup[e].Position;
        var parentPosition = WorldTransformLookup[parent.Value].Position;
        var distance = math.distance(myPosition, parentPosition);
        var direction = math.normalize(myPosition - parentPosition);

        var myVelocity = new float3(0, 0, 0);
        if (PhysicsLookup.TryGetComponent(e, out var myPhysics)) myVelocity = myPhysics.Velocity;

        var parentVelocity = new float3(0, 0, 0);
        if (PhysicsLookup.TryGetComponent(e, out var parentPhysics)) parentVelocity = parentPhysics.Velocity;

        var displacement = length.Length - distance;
        var displacementSpeed = math.dot(myVelocity - parentVelocity, direction);

        constraint.ValueRW.VelocityAdjustment -= direction * displacementSpeed;
        constraint.ValueRW.PositionAdjustment += direction * displacement;
    }
}

[BurstCompile]
public partial struct ResolveConstraints : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<ConstraintResponse> constraint,
                         RefRW<PhysicsBody> physics,
                         RefRW<LocalTransform> localTransform,
                         LocalToWorld worldTransform)
    {
        localTransform.ValueRW.Position += math.mul(math.inverse(worldTransform.Rotation), constraint.ValueRO.PositionAdjustment);
        physics.ValueRW.Velocity += constraint.ValueRO.VelocityAdjustment;

        constraint.ValueRW.PositionAdjustment = float3.zero;
        constraint.ValueRW.VelocityAdjustment = float3.zero;
    }
}

[BurstCompile]
public partial struct ResolveFaceDirectionConstraint : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<LocalTransform> localTransform,
                         LocalToWorld worldTransform,
                         FaceAwayFromParentConstraint constraint)
    {
        var back = quaternion.LookRotationSafe(-localTransform.ValueRO.Position, math.normalize(worldTransform.Position));
        localTransform.ValueRW.Rotation = math.mul(back, constraint.InitialRotation);
    }
}