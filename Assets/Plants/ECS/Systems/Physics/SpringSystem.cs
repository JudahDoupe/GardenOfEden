using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(ConstraintSystem))]
[BurstCompile]
public partial struct SpringSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new AddSpringForces()
            .Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct AddSpringForces : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics, RefRO<StiffSpringJoint> spring, TransformAspect transform)
    {
        var springForce = -spring.ValueRO.Stiffness * (transform.LocalPosition - spring.ValueRO.EquilibriumPosition);
        var dampingForce = -spring.ValueRO.Dampening * physics.ValueRO.Velocity;
        physics.ValueRW.Force += springForce + dampingForce;

        var back = quaternion.LookRotationSafe(-transform.LocalPosition, math.normalize(transform.WorldPosition));
        transform.LocalRotation = math.mul(back, spring.ValueRO.TargetRotation);
    }
}