using Framework.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

[BurstCompile]
[UpdateAfter(typeof(IntegrateVelocityEuler))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    public ComponentLookup<SphereCollider> SphereColliderLookup;
    public ComponentLookup<CapsuleCollider> CapsuleColliderLookup;
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    public ComponentLookup<LocalToWorld> WorldTransformLookup;
    public ComponentLookup<Parent> ParentLookup;
    public ComponentLookup<LocalTransform> LocalTransformLookup;
    public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
    public BufferLookup<Child> ChildrenLookup;
    public EntityQuery SphereQuery;
    public EntityQuery CapsulesQuery;
    public EntityQuery RecalculateLocalTransformQuery;

    private bool _haveTransformsInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        SphereColliderLookup = state.GetComponentLookup<SphereCollider>();
        CapsuleColliderLookup = state.GetComponentLookup<CapsuleCollider>();
        PhysicsLookup = state.GetComponentLookup<PhysicsBody>();
        WorldTransformLookup = state.GetComponentLookup<LocalToWorld>();
        ParentLookup = state.GetComponentLookup<Parent>();
        LocalTransformLookup = state.GetComponentLookup<LocalTransform>();
        PostTransformMatrixLookup = state.GetComponentLookup<PostTransformMatrix>();
        ChildrenLookup = state.GetBufferLookup<Child>();
        SphereQuery = state.GetEntityQuery(typeof(SphereCollider), typeof(PhysicsBody), typeof(LocalToWorld), typeof(CollisionResponse));
        CapsulesQuery = state.GetEntityQuery(typeof(CapsuleCollider), typeof(PhysicsBody), typeof(LocalToWorld), typeof(CollisionResponse));
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

        SphereColliderLookup.Update(ref state);
        CapsuleColliderLookup.Update(ref state);
        PhysicsLookup.Update(ref state);
        WorldTransformLookup.Update(ref state);
        ParentLookup.Update(ref state);
        LocalTransformLookup.Update(ref state);
        PostTransformMatrixLookup.Update(ref state);
        ChildrenLookup.Update(ref state);

        state.Dependency = new DetectSphereToGroundCollisions()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new DetectCapsuleToGroundCollisions()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new DetectSphereToSphereCollisions
            {
                ColliderLookup = SphereColliderLookup,
                PhysicsLookup = PhysicsLookup,
                WorldTransformLookup = WorldTransformLookup,
                ParentLookup = ParentLookup,
                ChildrenLookup = ChildrenLookup,
                Spheres = SphereQuery.ToEntityArray(Allocator.TempJob)
            }.ScheduleParallel(SphereQuery, state.Dependency);

        state.Dependency = new DetectCapsuleToCapsuleCollisions
        {
            ColliderLookup = CapsuleColliderLookup,
            PhysicsLookup = PhysicsLookup,
            WorldTransformLookup = WorldTransformLookup,
            ParentLookup = ParentLookup,
            ChildrenLookup = ChildrenLookup,
            Capsules = CapsulesQuery.ToEntityArray(Allocator.TempJob)
        }.ScheduleParallel(CapsulesQuery, state.Dependency);

        state.Dependency = new ResolveCollisions()
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
public partial struct DetectSphereToGroundCollisions : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRO<PhysicsBody> physics,
                         RefRO<SphereCollider> collider,
                         RefRO<LocalToWorld> worldTransform,
                         CollisionAspect collision)
    {
        var groundPosition = new float3(worldTransform.ValueRO.Position.x, 0.5f, worldTransform.ValueRO.Position.z);

        collision.AddSphereToGroundCollisionResponse(worldTransform.ValueRO.Position,
                                                     groundPosition,
                                                     collider.ValueRO.Radius,
                                                     physics.ValueRO.Velocity,
                                                     collider.ValueRO.Bounciness);
    }
}

[BurstCompile]
public partial struct DetectCapsuleToGroundCollisions : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRO<PhysicsBody> physics,
                         RefRO<CapsuleCollider> collider,
                         RefRO<LocalToWorld> worldTransform,
                         CollisionAspect collision)
    {
        var myStart = worldTransform.ValueRO.Position + math.mul(worldTransform.ValueRO.Rotation, collider.ValueRO.Start);
        var myEnd = worldTransform.ValueRO.Position + math.mul(worldTransform.ValueRO.Rotation, collider.ValueRO.End);
        var groundStart = new float3(myStart.x, 0.5f, myStart.z);
        var groundEnd = new float3(myEnd.x, 0.5f, myEnd.z);

        var (myClosestPoint, groundPosition) = collision.ClosestPointsOnLineSegments(myStart, groundStart, myEnd, groundEnd);
        collision.AddSphereToGroundCollisionResponse(myClosestPoint,
                                                     groundPosition,
                                                     collider.ValueRO.Radius,
                                                     physics.ValueRO.Velocity,
                                                     collider.ValueRO.Bounciness);
    }
}

[BurstCompile]
public partial struct DetectSphereToSphereCollisions : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<SphereCollider> ColliderLookup;
    [ReadOnly]
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    [ReadOnly]
    public ComponentLookup<LocalToWorld> WorldTransformLookup;
    [ReadOnly]
    public NativeArray<Entity> Spheres;
    [ReadOnly]
    public ComponentLookup<Parent> ParentLookup;
    [ReadOnly]
    public BufferLookup<Child> ChildrenLookup;

    [BurstCompile]
    private void Execute(Entity e, CollisionAspect collision)
    {
        var myCollider = ColliderLookup[e];
        var myTransform = WorldTransformLookup[e];
        var myPhysics = PhysicsLookup[e];
        foreach (var sphere in Spheres)
        {
            if (!collision.ShouldCollide(e, sphere, ParentLookup, ChildrenLookup))
                continue;

            var otherCollider = ColliderLookup[sphere];
            var otherTransform = WorldTransformLookup[sphere];
            var otherPhysics = PhysicsLookup[sphere];

            collision.AddSphereToSphereCollisionResponse(myTransform.Position,
                                                         otherTransform.Position,
                                                         myCollider.Radius,
                                                         otherCollider.Radius,
                                                         myPhysics.Velocity,
                                                         otherPhysics.Velocity,
                                                         myCollider.Bounciness,
                                                         otherCollider.Bounciness);
        }
    }
}

[BurstCompile]
public partial struct DetectCapsuleToCapsuleCollisions : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<CapsuleCollider> ColliderLookup;
    [ReadOnly]
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    [ReadOnly]
    public ComponentLookup<LocalToWorld> WorldTransformLookup;
    [ReadOnly]
    public NativeArray<Entity> Capsules;
    [ReadOnly]
    public ComponentLookup<Parent> ParentLookup;
    [ReadOnly]
    public BufferLookup<Child> ChildrenLookup;

    [BurstCompile]
    private void Execute(Entity e, CollisionAspect collision)
    {
        var myCollider = ColliderLookup[e];
        var myTransform = WorldTransformLookup[e];
        var myPhysics = PhysicsLookup[e];
        foreach (var capsule in Capsules)
        {
            if (!collision.ShouldCollide(e, capsule, ParentLookup, ChildrenLookup))
                continue;

            var otherCollider = ColliderLookup[capsule];
            var otherTransform = WorldTransformLookup[capsule];
            var otherPhysics = PhysicsLookup[capsule];

            var myStart = myTransform.Position + math.mul(myTransform.Rotation, myCollider.Start);
            var myEnd = myTransform.Position + math.mul(myTransform.Rotation, myCollider.End);
            var otherStart = otherTransform.Position + math.mul(otherTransform.Rotation, otherCollider.Start);
            var otherEnd = otherTransform.Position + math.mul(otherTransform.Rotation, otherCollider.End);

            var (myClosestPoint, otherClosestPoint) = collision.ClosestPointsOnLineSegments(myStart, otherStart, myEnd, otherEnd);
            collision.AddSphereToSphereCollisionResponse(myClosestPoint,
                                                         otherClosestPoint,
                                                         myCollider.Radius,
                                                         otherCollider.Radius,
                                                         myPhysics.Velocity,
                                                         otherPhysics.Velocity,
                                                         myCollider.Bounciness,
                                                         otherCollider.Bounciness);
        }
    }
}

[BurstCompile]
public partial struct ResolveCollisions : IJobEntity
{
    [BurstCompile]
    private void Execute(Entity e,
                         RefRW<PhysicsBody> physics,
                         RefRW<LocalTransform> transform,
                         LocalToWorld worldTransform,
                         CollisionAspect collision)
    {
        transform.ValueRW.Position += math.mul(math.inverse(worldTransform.Rotation), collision.PositionAdjustment);
        transform.ValueRW.Position += collision.PositionAdjustment;
        physics.ValueRW.Velocity += collision.VelocityAdjustment;

        collision.Clear();
    }
}