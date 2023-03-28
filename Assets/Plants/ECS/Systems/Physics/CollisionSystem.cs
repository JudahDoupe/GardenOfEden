using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(VelocityIntegrationSystem))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    public ComponentLookup<SphereCollider> ColliderLookup;
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    public ComponentLookup<WorldTransform> TransformLookup;
    public EntityQuery SphereQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        ColliderLookup = state.GetComponentLookup<SphereCollider>();
        PhysicsLookup = state.GetComponentLookup<PhysicsBody>();
        TransformLookup = state.GetComponentLookup<WorldTransform>();
        SphereQuery = state.GetEntityQuery(typeof(SphereCollider));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.fixedDeltaTime;
        ColliderLookup.Update(ref state);
        PhysicsLookup.Update(ref state);
        TransformLookup.Update(ref state);

        state.Dependency = new DetectGroundCollisions
        {
            TimeStep = deltaTime
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new DetectSphereToSphereCollisions
        {
            TimeStep = deltaTime,
            ColliderLookup = ColliderLookup,
            PhysicsLookup = PhysicsLookup,
            TransformLookup = TransformLookup,
            Spheres = SphereQuery.ToEntityArray(Allocator.TempJob)
        }.ScheduleParallel(SphereQuery, state.Dependency);

        state.Dependency = new ResolveCollisions()
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct DetectGroundCollisions : IJobEntity
{
    public float TimeStep;

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
        var penetrationSpeed = math.dot(physics.ValueRO.Velocity, penetrationNormal);
        var penetrationVector = penetrationNormal * penetrationSpeed;
        var restitution = 1 + collider.ValueRO.Bounciness;

        collision.ValueRW.VelocityAdjustment -= penetrationVector * restitution;
        collision.ValueRW.PositionAdjustment += penetrationNormal * overlap;
    }
}

[BurstCompile]
public partial struct DetectSphereToSphereCollisions : IJobEntity
{
    public float TimeStep;

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
                         RefRW<CollisionResponse> collision)
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

            var distance = math.distance(myTransform.Position, otherTransform.Position);
            var overlap = myCollider.Radius + otherCollider.Radius - distance;
            if (overlap < 0)
                continue;

            var penetrationNormal = math.normalize(myTransform.Position - otherTransform.Position);
            var penetrationSpeed = math.dot(myPhysics.Velocity - otherPhysics.Velocity, penetrationNormal);
            var penetrationVector = penetrationNormal * penetrationSpeed;
            var restitution = 1 + math.max(myCollider.Bounciness, otherCollider.Bounciness);

            collision.ValueRW.VelocityAdjustment -= penetrationVector * restitution;
            collision.ValueRW.PositionAdjustment += penetrationNormal * overlap;
        }
    }
}

[BurstCompile]
public partial struct ResolveCollisions : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics,
                         TransformAspect transform,
                         RefRW<CollisionResponse> collision)
    {
        transform.WorldPosition += collision.ValueRO.PositionAdjustment;
        physics.ValueRW.Velocity += collision.ValueRO.VelocityAdjustment;

        collision.ValueRW.PositionAdjustment = float3.zero;
        collision.ValueRW.VelocityAdjustment = float3.zero;
    }
}