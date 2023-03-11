using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public readonly partial struct PrimaryGrowthAspect : IAspect
{
    private readonly RefRO<PrimaryGrowth> _primaryGrowthTarget;
    private readonly RefRW<Size> _size;
    private readonly RefRW<PhysicsMass> _mass;

    public bool IsMature => (MaxNodeRadius * 0.99) < NodeRadius;
    public float MaxNodeRadius => _primaryGrowthTarget.ValueRO.NodeRadius;
    public float MaxInternodeLength => _primaryGrowthTarget.ValueRO.InternodeLength;

    public float Volume => math.pow(NodeRadius, 3) + (InternodeLength * math.pow(NodeRadius, 2));
    public float Density => _primaryGrowthTarget.ValueRO.Density;
    public float Mass => 1 / _mass.ValueRO.InverseMass;

    public float NodeRadius
    {
        get => _size.ValueRO.NodeRadius;
        set
        {
            _size.ValueRW.NodeRadius = value;
            _mass.ValueRW.InverseMass = 1 / math.max(Volume * Density, 0.0001f);
        }
    }

    public float InternodeLength
    {
        get => _size.ValueRO.InternodeLength;
        set
        {
            _size.ValueRW.InternodeLength = value;
            _mass.ValueRW.InverseMass = 1 / math.max(Volume * Density, 0.0001f);
        }
    }
}