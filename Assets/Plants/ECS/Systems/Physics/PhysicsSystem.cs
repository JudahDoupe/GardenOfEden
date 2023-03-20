using System.Threading;
using Unity.Burst;
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

        state.Dependency = new AddGravity().ScheduleParallel(state.Dependency);
        state.Dependency = new IntegrateVelocityEuler
        {
            TimeStep = deltaTime,
        }.ScheduleParallel(state.Dependency);
        state.Dependency = new DetectGroundCollisions
        {
            TimeStep = deltaTime,
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct AddGravity : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<Physics> physics)
    {
        physics.ValueRW.Force += new float3(0, -9.8f, 0) * physics.ValueRO.Mass;
    }
}

[BurstCompile]
public partial struct IntegrateVelocityEuler : IJobEntity
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
public partial struct DetectGroundCollisions : IJobEntity
{
    public float TimeStep;
    
    [BurstCompile]
    private void Execute(RefRW<Physics> physics, RefRO<SphereCollider> collider, TransformAspect transform)
    {
        var overlap = 0.5f - (transform.WorldPosition.y - collider.ValueRO.Radius);
        if (overlap < 0) 
            return; //no collision
        
        var penetrationNormal = new float3(0, 1, 0);
        var penetrationSpeed = math.dot(physics.ValueRW.Velocity, penetrationNormal);
        var penetrationVector = penetrationNormal * penetrationSpeed;
        var restitution = 1 + collider.ValueRO.Bounciness;
        physics.ValueRW.Velocity -= penetrationVector * restitution;
        transform.WorldPosition += penetrationNormal * overlap;
    }
}

[BurstCompile]
public partial struct DetectSphereToSphereCollisions : IJobEntity
{
    public float TimeStep;
    
    [BurstCompile]
    private void Execute(RefRW<Physics> physics, RefRO<SphereCollider> collider, TransformAspect transform)
    {
        var myPosition = transform.WorldPosition;
        var otherPosition = new float3(transform.WorldPosition.x, 0.5f, transform.WorldPosition.z);
        var distanceVector = myPosition - otherPosition;

        if (math.lengthsq(distanceVector) > math.sqrt(collider.ValueRO.Radius)) 
            return; //no collision

        var separationVector = distanceVector - (math.normalize(distanceVector) * collider.ValueRO.Radius);
        
        transform.WorldPosition += separationVector;
        physics.ValueRW.Velocity += separationVector;        
    }
}
