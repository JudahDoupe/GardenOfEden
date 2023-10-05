using Framework.Jobs;
using Framework.Utils;
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
    public ComponentLookup<LocalToWorld> WorldTransformLookup;
    public ComponentLookup<Parent> ParentLookup;
    public ComponentLookup<LocalTransform> LocalTransformLookup;
    public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
    public ComponentLookup<FaceAwayFromParentConstraint> FaceAwayLookup;
    public EntityQuery RecalculateLocalTransformQuery;

    private bool _haveTransformsInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        WorldTransformLookup = state.GetComponentLookup<LocalToWorld>();
        ParentLookup = state.GetComponentLookup<Parent>();
        LocalTransformLookup = state.GetComponentLookup<LocalTransform>();
        PostTransformMatrixLookup = state.GetComponentLookup<PostTransformMatrix>();
        FaceAwayLookup = state.GetComponentLookup<FaceAwayFromParentConstraint>();
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

        WorldTransformLookup.Update(ref state);
        ParentLookup.Update(ref state);
        LocalTransformLookup.Update(ref state);
        PostTransformMatrixLookup.Update(ref state);
        FaceAwayLookup.Update(ref state);

        state.Dependency = new SolveLengthConstraint
            {
                WorldTransformLookup = WorldTransformLookup
            }
            .ScheduleParallel(state.Dependency);

        state.Dependency = new ResolveFaceDirectionConstraint()
            .ScheduleParallel(state.Dependency);
        
        state.Dependency = new ResolveFaceDirectionConstraintForChildren()
            {
                FaceAwayLookup = FaceAwayLookup,
            }
            .ScheduleParallel(state.Dependency);

        state.Dependency = new ResolveConstraints
            {
                TimeStep = SystemAPI.Time.fixedDeltaTime,
            }
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

        var displacement = length.Length - distance;

        var positionAdjustment = direction * displacement;
        constraint.ValueRW.PositionAdjustment += positionAdjustment;
    }
}

[BurstCompile]
public partial struct ResolveConstraints : IJobEntity
{
    public float TimeStep;
    
    [BurstCompile]
    private void Execute(RefRW<ConstraintResponse> constraint,
                         RefRW<PhysicsBody> physics,
                         RefRW<LocalTransform> localTransform,
                         LocalToWorld worldTransform)
    {
        localTransform.ValueRW.TranslateWorld(worldTransform, constraint.ValueRO.PositionAdjustment);
        physics.ValueRW.Velocity += constraint.ValueRO.PositionAdjustment / TimeStep;

        constraint.ValueRW.PositionAdjustment = float3.zero;
    }
}

[BurstCompile]
public partial struct ResolveFaceDirectionConstraint : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<LocalTransform> localTransform,
                         LocalToWorld worldTransform,
                         RefRW<FaceAwayFromParentConstraint> constraint)
    {
        var back = quaternion.LookRotationSafe(-localTransform.ValueRO.Position, math.normalize(worldTransform.Position));
        var newRotation = math.mul(back, constraint.ValueRO.InitialRotation);
        constraint.ValueRW.RotationAdjustment = math.mul(localTransform.ValueRW.Rotation, math.inverse(newRotation));
        localTransform.ValueRW.Rotation = newRotation;
    }
}

[BurstCompile]
public partial struct ResolveFaceDirectionConstraintForChildren : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<FaceAwayFromParentConstraint> FaceAwayLookup;
    
    [BurstCompile]
    private void Execute(RefRW<LocalTransform> localTransform,
                         Parent parent,
                         PhysicsBody _)
    {
        if (FaceAwayLookup.HasComponent(parent.Value))
        {
            localTransform.ValueRW.Rotation = math.mul(localTransform.ValueRW.Rotation, FaceAwayLookup[parent.Value].RotationAdjustment);
        }
    }
}