using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct GrowthAspect : IAspect
{
    private readonly PhysicsAspect _physics;
    private readonly RefRW<PrimaryGrowth> _primaryGrowthTarget;
    private readonly RefRW<Size> _size;

    public bool IsMature => MaxNodeRadius * 0.99 < NodeRadius;
    public float MaxNodeRadius => _primaryGrowthTarget.ValueRO.NodeRadius;
    public float MaxInternodeLength => _primaryGrowthTarget.ValueRO.InternodeLength;

    public float NodeRadius
    {
        get => _size.ValueRO.NodeRadius;
        set => _size.ValueRW.NodeRadius = value;
    }

    public float InternodeLength
    {
        get => _size.ValueRO.InternodeLength;
        set => _size.ValueRW.InternodeLength = value;
    }


    public LocalTransform LocalTransform => _physics.Transform.LocalTransform;
    public float3 LocalPosition
    {
        get => _physics.Transform.LocalPosition;
        set => _physics.Transform.LocalPosition = value;
    }

    public void Grow(float energy)
    {
        var requestedNodeEnergy = math.min(energy, MaxNodeRadius - NodeRadius);
        NodeRadius += requestedNodeEnergy;

        energy -= requestedNodeEnergy;

        var requestedInternodeEnergy = math.min(energy, MaxInternodeLength - InternodeLength);
        InternodeLength += requestedInternodeEnergy;
        
        _physics.EquilibriumPosition = _physics.Transform.LocalTransform.Forward() * InternodeLength;
        _physics.Collider.ValueRW.Radius = NodeRadius;
    }
}