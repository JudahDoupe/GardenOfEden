using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

[UpdateAfter(typeof(VelocityIntegrationSystem))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    public ComponentLookup<SphereCollider> SphereColliderLookup;
    public ComponentLookup<CapsuleCollider> CapsuleColliderLookup;
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    public ComponentLookup<WorldTransform> TransformLookup;
    public ComponentLookup<Parent> ParentLookup;
    public BufferLookup<Child> ChildrenLookup;
    public EntityQuery SphereQuery;
    public EntityQuery CapsulesQuery;

    private bool _haveTransformsInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        SphereColliderLookup = state.GetComponentLookup<SphereCollider>();
        CapsuleColliderLookup = state.GetComponentLookup<CapsuleCollider>();
        PhysicsLookup = state.GetComponentLookup<PhysicsBody>();
        TransformLookup = state.GetComponentLookup<WorldTransform>();
        ParentLookup = state.GetComponentLookup<Parent>();
        ChildrenLookup = state.GetBufferLookup<Child>();
        SphereQuery = state.GetEntityQuery(typeof(SphereCollider));
        CapsulesQuery = state.GetEntityQuery(typeof(CapsuleCollider));
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
        TransformLookup.Update(ref state);
        ParentLookup.Update(ref state);
        ChildrenLookup.Update(ref state);

        state.Dependency = new DetectSphereToGroundCollisions()
            .ScheduleParallel(state.Dependency);
        
        state.Dependency = new DetectCapsuleToGroundCollisions()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new DetectSphereToSphereCollisions
        {
            ColliderLookup = SphereColliderLookup,
            PhysicsLookup = PhysicsLookup,
            TransformLookup = TransformLookup,
            ParentLookup = ParentLookup,
            ChildrenLookup = ChildrenLookup,
            Spheres = SphereQuery.ToEntityArray(Allocator.TempJob)
        }.ScheduleParallel(SphereQuery, state.Dependency);
        
        state.Dependency = new DetectCapsuleToCapsuleCollisions()
        {
            ColliderLookup = CapsuleColliderLookup,
            PhysicsLookup = PhysicsLookup,
            TransformLookup = TransformLookup,
            ParentLookup = ParentLookup,
            ChildrenLookup = ChildrenLookup,
            Capsules = CapsulesQuery.ToEntityArray(Allocator.TempJob)
        }.ScheduleParallel(CapsulesQuery, state.Dependency);

        state.Dependency = new ResolveCollisions()
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct DetectSphereToGroundCollisions : IJobEntity
{

    [BurstCompile]
    private void Execute(RefRO<PhysicsBody> physics,
                         RefRO<SphereCollider> collider,
                         RefRO<WorldTransform> transform,
                         CollisionAspect collision)
    {
        var groundPosition = new float3(transform.ValueRO.Position.x, 0.5f, transform.ValueRO.Position.z);

        collision.AddSphereToGroundCollisionResponse(transform.ValueRO.Position, 
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
                         RefRO<WorldTransform> transform,
                         CollisionAspect collision)
    {
        var myStart = transform.ValueRO.Position + math.mul(transform.ValueRO.Rotation, collider.ValueRO.Start);
        var myEnd = transform.ValueRO.Position + math.mul(transform.ValueRO.Rotation, collider.ValueRO.End);
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
    [ReadOnly] public ComponentLookup<SphereCollider> ColliderLookup;
    [ReadOnly] public ComponentLookup<PhysicsBody> PhysicsLookup;
    [ReadOnly] public ComponentLookup<WorldTransform> TransformLookup;
    [ReadOnly] public NativeArray<Entity> Spheres;
    [ReadOnly] public ComponentLookup<Parent> ParentLookup;
    [ReadOnly] public BufferLookup<Child> ChildrenLookup;

    [BurstCompile]
    private void Execute(Entity e, CollisionAspect collision)
    {
        var myCollider = ColliderLookup[e];
        var myTransform = TransformLookup[e];
        var myPhysics = PhysicsLookup[e];
        foreach (var sphere in Spheres)
        {
            if (!collision.ShouldCollide(e, sphere, ParentLookup, ChildrenLookup))
                continue;

            var otherCollider = ColliderLookup[sphere];
            var otherTransform = TransformLookup[sphere];
            var otherPhysics = PhysicsLookup[sphere];

            collision.AddSphereToSphereCollisionResponse(myTransform.Position, otherTransform.Position,
                                                         myCollider.Radius, otherCollider.Radius,
                                                         myPhysics.Velocity, otherPhysics.Velocity,
                                                         myCollider.Bounciness, otherCollider.Bounciness);
        }
    }
}

[BurstCompile]
public partial struct DetectCapsuleToCapsuleCollisions : IJobEntity
{
    [ReadOnly] public ComponentLookup<CapsuleCollider> ColliderLookup;
    [ReadOnly] public ComponentLookup<PhysicsBody> PhysicsLookup;
    [ReadOnly] public ComponentLookup<WorldTransform> TransformLookup;
    [ReadOnly] public NativeArray<Entity> Capsules;
    [ReadOnly] public ComponentLookup<Parent> ParentLookup;
    [ReadOnly] public BufferLookup<Child> ChildrenLookup;

    [BurstCompile]
    private void Execute(Entity e, CollisionAspect collision)
    {
        var myCollider = ColliderLookup[e];
        var myTransform = TransformLookup[e];
        var myPhysics = PhysicsLookup[e];
        foreach (var capsule in Capsules)
        {
            if (!collision.ShouldCollide(e, capsule, ParentLookup, ChildrenLookup))
                continue;

            var otherCollider = ColliderLookup[capsule];
            var otherTransform = TransformLookup[capsule];
            var otherPhysics = PhysicsLookup[capsule];
            
            var myStart = myTransform.Position + math.mul(myTransform.Rotation, myCollider.Start);
            var myEnd = myTransform.Position + math.mul(myTransform.Rotation, myCollider.End);
            var otherStart = otherTransform.Position + math.mul(otherTransform.Rotation, otherCollider.Start);
            var otherEnd = otherTransform.Position + math.mul(otherTransform.Rotation, otherCollider.End);

            var (myClosestPoint, otherClosestPoint) = collision.ClosestPointsOnLineSegments(myStart, otherStart, myEnd, otherEnd);
            collision.AddSphereToSphereCollisionResponse(myClosestPoint, otherClosestPoint,
                                                         myCollider.Radius, otherCollider.Radius,
                                                         myPhysics.Velocity, otherPhysics.Velocity,
                                                         myCollider.Bounciness, otherCollider.Bounciness);
        }
    }
}

[BurstCompile]
public partial struct ResolveCollisions : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics,
                         TransformAspect transform,
                         CollisionAspect collision)
    {
        transform.WorldPosition += collision.PositionAdjustment;
        physics.ValueRW.Velocity += collision.VelocityAdjustment;

        collision.Clear();
    }
}