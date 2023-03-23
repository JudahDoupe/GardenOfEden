using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct PhysicsSystem : ISystem
{
    public ComponentLookup<SphereCollider> ColliderLookup;
    public ComponentLookup<PhysicsBody> PhysicsLookup;
    public ComponentLookup<WorldTransform> TransformLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        ColliderLookup = state.GetComponentLookup<SphereCollider>();
        PhysicsLookup = state.GetComponentLookup<PhysicsBody>();
        TransformLookup = state.GetComponentLookup<WorldTransform>();
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

        state.Dependency = new AddGravity()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new IntegrateVelocityEuler
        {
            TimeStep = deltaTime,
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new DetectGroundCollisions
        {
            TimeStep = deltaTime,
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new DetectSphereToSphereCollisions
        {
            TimeStep = deltaTime,
            ColliderLookup = ColliderLookup,
            PhysicsLookup = PhysicsLookup,
            TransformLookup = TransformLookup,
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new ResolveCollisions()
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct AddGravity : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics)
    {
        physics.ValueRW.Force += new float3(0, -9.8f, 0) * physics.ValueRO.Mass;
    }
}

[BurstCompile]
public partial struct IntegrateVelocityEuler : IJobEntity
{
    public float TimeStep;

    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics, 
                         TransformAspect transform)
    {
        transform.WorldPosition += physics.ValueRO.Velocity * TimeStep;

        physics.ValueRW.Velocity += physics.ValueRO.Force / physics.ValueRO.Mass * TimeStep;
        physics.ValueRW.Force = 0;
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
        var overlap = 0.5f - (transform.ValueRO.Position.y - collider.ValueRO.Radius);
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


    [BurstCompile]
    private void Execute(Entity e, 
                         RefRW<CollisionResponse> collision)
    {
        return;
        //Entity query
        /*
        var myPosition = transform.WorldPosition;
        var otherPosition = new float3(transform.WorldPosition.x, 0.5f, transform.WorldPosition.z);
        var distanceVector = myPosition - otherPosition;

        if (math.lengthsq(distanceVector) > math.sqrt(ColliderLookup[e].Radius))
            return; //no collision

        var separationVector = distanceVector - math.normalize(distanceVector) * ColliderLookup[e].Radius;

        transform.WorldPosition += separationVector;
        physics.ValueRW.Velocity += separationVector;
        */
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