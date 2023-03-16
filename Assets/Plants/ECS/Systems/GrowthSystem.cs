using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[UpdateInGroup(typeof(PlantSimulationGroup))]
[UpdateAfter(typeof(ReplicationSystem))]
public partial struct GrowthSystem : ISystem
{
    private ComponentLookup<Size> _sizeLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _sizeLookup = state.GetComponentLookup<Size>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        _sizeLookup.Update(ref state);

        state.Dependency = new PrimaryGrowthJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(state.Dependency);

        state.Dependency = new UpdatePhysicsJob
        {
            SizeLookup = _sizeLookup,
        }.ScheduleParallel(state.Dependency);
    }
}

public partial struct PrimaryGrowthJob : IJobEntity
{
    public float DeltaTime;

    [BurstCompile]
    private void Execute(RefRW<PhysicsCollider> spring, PrimaryGrowthAspect primaryGrowth)
    {
        var energy = DeltaTime / 10;
        
        var requestedNodeEnergy = math.min(energy, primaryGrowth.MaxNodeRadius - primaryGrowth.NodeRadius);
        primaryGrowth.NodeRadius += requestedNodeEnergy;

        energy -= requestedNodeEnergy;

        var requestedInternodeEnergy = math.min(energy, primaryGrowth.MaxInternodeLength - primaryGrowth.InternodeLength);
        primaryGrowth.InternodeLength += requestedInternodeEnergy;
        
    }
}

public partial struct UpdatePhysicsJob : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<Size> SizeLookup;

    [BurstCompile]
    private void Execute(RefRW<Spring> spring, RefRW<PhysicsJoint> joint, RefRO<PhysicsConstrainedBodyPair> bodyPair)
    {
        var nodeEntity = bodyPair.ValueRO.EntityA;

        if (!SizeLookup.HasComponent(nodeEntity))
            return;

        spring.ValueRW.EquilibriumPosition = SizeLookup[nodeEntity].LocalDirection * SizeLookup[nodeEntity].InternodeLength;
        
        var frame = joint.ValueRW.BodyAFromJoint;
        frame.Position = new float3(0, 0, -SizeLookup[nodeEntity].InternodeLength);
        joint.ValueRW.BodyAFromJoint = frame;
    }
}