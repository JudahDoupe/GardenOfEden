using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(ConstraintSystem))]
[BurstCompile]
public partial struct SpringSystem : ISystem
{
    private bool _haveTransformsInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _haveTransformsInitialized = false;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!_haveTransformsInitialized)
        {
            _haveTransformsInitialized = true;
            return;
        }

        state.Dependency = new AddSpringForces()
            .Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct AddSpringForces : IJobEntity
{
    [BurstCompile]
    private void Execute(RefRW<PhysicsBody> physics, 
                         RefRO<SpringJoint> spring, 
                         RefRW<LocalTransform> localTransform, 
                         LocalToWorld worldTransform)
    {
        var springForce = -spring.ValueRO.Stiffness * (localTransform.ValueRO.Position - spring.ValueRO.EquilibriumPosition);
        var dampingForce = -spring.ValueRO.Dampening * physics.ValueRO.Velocity;
        physics.ValueRW.Force += springForce + dampingForce;

        var back = quaternion.LookRotationSafe(-localTransform.ValueRO.Position, math.normalize(worldTransform.Position));
        localTransform.ValueRW.Rotation = math.mul(back, spring.ValueRO.TargetRotation);
    }
}