using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct PhysicsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.fixedDeltaTime;

        state.Dependency = new AddGravity()
            .ScheduleParallel(state.Dependency);

        state.Dependency = new IntegrateVelocityEuler
        {
            TimeStep = deltaTime
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new DetectGroundCollisions
        {
            TimeStep = deltaTime
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new DetectSphereToSphereCollisions
        {
            TimeStep = deltaTime
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new ResolveCollisions()
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public struct AddGravity : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<Physics> physics)
    {
        physics.ValueRW.Force += new float3(0, -9.8f, 0) * physics.ValueRO.Mass;
    }
}

[BurstCompile]
public struct IntegrateVelocityEuler : IJobEntity
{
    public float TimeStep;

    [BurstCompile]
    private void Execute(RefRW<Physics> physics, TransformAspect transform)
    {
        transform.WorldPosition += physics.ValueRO.Velocity * TimeStep;

        physics.ValueRW.Velocity += physics.ValueRO.Force / physics.ValueRO.Mass * TimeStep;
        physics.ValueRW.Force = 0;
    }
}

[BurstCompile]
public struct DetectGroundCollisions : IJobEntity
{
    public float TimeStep;

    [BurstCompile]
    private void Execute(RefRO<Physics> physics, RefRO<SphereCollider> collider, RefRO<WorldTransform> transform, RefRW<Collision> collision)
    {
        var overlap = 0.5f - (transform.ValueRO.Position.y - collider.ValueRO.Radius);
        if (overlap < 0)
            return;

        var penetrationNormal = new float3(0, 1, 0);
        var penetrationSpeed = math.dot(physics.ValueRO.Velocity, penetrationNormal);
        var penetrationVector = penetrationNormal * penetrationSpeed;
        var restitution = 1 + collider.ValueRO.Bounciness;

        collision.ValueRW.VelocityAdjustment -= penetrationVector * restitution;
        collision.ValueRW.VelocityAdjustment += penetrationNormal * overlap;
    }
}

[BurstCompile]
public struct DetectSphereToSphereCollisions : IJobEntity
{
    public float TimeStep;

    [ReadOnly]
    public ComponentLookup<SphereCollider> ColliderLookup;
    [ReadOnly]
    public ComponentLookup<Physics> PhysicsLookup;
    [ReadOnly]
    public ComponentLookup<WorldTransform> TransformLookup;


    [BurstCompile]
    private void Execute(Entity e, RefRW<Collision> collision)
    {
        return;
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
public struct ResolveCollisions : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<Physics> physics, TransformAspect transform, RefRW<Collision> collision)
    {
        transform.WorldPosition += collision.ValueRO.PositionAdjustment;
        physics.ValueRW.Velocity += collision.ValueRO.VelocityAdjustment;

        collision.ValueRW.PositionAdjustment = float3.zero;
        collision.ValueRW.VelocityAdjustment = float3.zero;
    }
}