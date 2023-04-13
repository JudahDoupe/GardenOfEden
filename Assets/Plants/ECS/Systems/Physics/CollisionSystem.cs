using Framework.Utils;
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

        state.Dependency = new DetectGroundCollisions()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new DetectSphereToSphereCollisions
        {
            ColliderLookup = SphereColliderLookup,
            PhysicsLookup = PhysicsLookup,
            TransformLookup = TransformLookup,
            Spheres = SphereQuery.ToEntityArray(Allocator.TempJob)
        }.ScheduleParallel(SphereQuery, state.Dependency);
        
        state.Dependency = new DetectCapsuleToCapsuleCollisions()
        {
            ColliderLookup = CapsuleColliderLookup,
            PhysicsLookup = PhysicsLookup,
            TransformLookup = TransformLookup,
            Capsules = CapsulesQuery.ToEntityArray(Allocator.TempJob)
        }.ScheduleParallel(CapsulesQuery, state.Dependency);

        state.Dependency = new ResolveCollisions()
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct DetectGroundCollisions : IJobEntity
{

    [BurstCompile]
    private void Execute(RefRO<PhysicsBody> physics,
                         RefRO<SphereCollider> collider,
                         RefRO<WorldTransform> transform,
                         RefRW<CollisionResponse> collision)
    {
        var overlap = 0.5f - transform.ValueRO.Position.y + collider.ValueRO.Radius;
        if (overlap < 0)
            return;

        var penetrationNormal = new float3(0, 1, 0);
        var penetrationSpeed = math.dot(physics.ValueRO.Velocity,
                                        penetrationNormal);
        var restitution = 1 + collider.ValueRO.Bounciness;

        collision.ValueRW.VelocityAdjustment -= penetrationNormal * penetrationSpeed * restitution;
        collision.ValueRW.PositionAdjustment += penetrationNormal * overlap;
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
    public ComponentLookup<WorldTransform> TransformLookup;
    [ReadOnly]
    public NativeArray<Entity> Spheres;


    [BurstCompile]
    private void Execute(Entity e,
                         CollisionAspect collision)
    {
        var myCollider = ColliderLookup[e];
        var myTransform = TransformLookup[e];
        var myPhysics = PhysicsLookup[e];
        foreach (var sphere in Spheres)
        {
            if (sphere == e)
                continue;

            var otherCollider = ColliderLookup[sphere];
            var otherTransform = TransformLookup[sphere];
            var otherPhysics = PhysicsLookup[sphere];

            collision.AddSphereCollisionResponse(myTransform.Position,
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
    public ComponentLookup<WorldTransform> TransformLookup;
    [ReadOnly]
    public NativeArray<Entity> Capsules;


    [BurstCompile]
    private void Execute(Entity e,
                         CollisionAspect collision)
    {
        var myCollider = ColliderLookup[e];
        var myTransform = TransformLookup[e];
        var myPhysics = PhysicsLookup[e];
        var myStart = myTransform.Position + math.mul(myTransform.Rotation, myCollider.Start);
        var myEnd = myTransform.Position + math.mul(myTransform.Rotation, myCollider.End);
        foreach (var capsule in Capsules)
        {
            if (capsule == e)
                continue;

            var otherCollider = ColliderLookup[capsule];
            var otherTransform = TransformLookup[capsule];
            var otherPhysics = PhysicsLookup[capsule];
            var otherStart = otherTransform.Position + math.mul(otherTransform.Rotation, otherCollider.Start);
            var otherEnd = otherTransform.Position + math.mul(otherTransform.Rotation, otherCollider.End);
            var v0 = otherStart - myStart;
            var v1 = otherEnd - myStart;
            var v2 = otherStart - myEnd;
            var v3 = otherEnd - myEnd;

            var d0 = math.dot(v0, v0);
            var d1 = math.dot(v1, v1);
            var d2 = math.dot(v2, v2);
            var d3 = math.dot(v3, v3);

            var myClosestPoint = d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1 ? myEnd : myStart;
            var otherClosestPoint = myClosestPoint.ClosestPointOnLineSegment(otherStart, otherEnd);
            myClosestPoint = otherClosestPoint.ClosestPointOnLineSegment(myStart, myEnd);
            
            collision.AddSphereCollisionResponse(myClosestPoint,
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
    private void Execute(RefRW<PhysicsBody> physics,
                         TransformAspect transform,
                         CollisionAspect collision)
    {
        transform.WorldPosition += collision.PositionAdjustment;
        physics.ValueRW.Velocity += collision.VelocityAdjustment;

        collision.Clear();
    }
}