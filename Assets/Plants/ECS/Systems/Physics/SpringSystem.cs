using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(VelocityIntegrationSystem))]
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
                         LocalTransform localTransform,
                         SpringJoint spring)
    {
        var springForce = -spring.Stiffness * (localTransform.Position - spring.EquilibriumPosition);
        var dampingForce = -spring.Dampening * physics.ValueRO.Velocity;
        physics.ValueRW.Force += springForce + dampingForce;
    }
}