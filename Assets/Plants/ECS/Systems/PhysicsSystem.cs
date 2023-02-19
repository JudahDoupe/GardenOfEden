using Unity.Burst;
using Unity.Entities;

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

        new UpdatePosition
        {
            DeltaTime = deltaTime
        }.Run();
    }
}

[BurstCompile]
public partial struct UpdatePosition : IJobEntity
{
    public float DeltaTime;

    [BurstCompile]
    private void Execute(PhysicsAspect physics)
    {
        physics.UpdatePosition(DeltaTime); 
    }
}