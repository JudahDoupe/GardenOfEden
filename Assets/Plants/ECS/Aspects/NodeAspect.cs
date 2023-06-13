using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct NodeAspect : IAspect
{
    private readonly RefRW<PrimaryGrowth> _primaryGrowthTarget;
    private readonly RefRW<Size> _size;

    public readonly NodePhysicsAspect Physics;

    public bool IsMature => MaxNodeRadius * 0.99 < NodeRadius;
    public float MaxNodeRadius => _primaryGrowthTarget.ValueRO.NodeRadius;
    public float MaxInternodeLength => _primaryGrowthTarget.ValueRO.InternodeLength;

    public float NodeRadius => _size.ValueRO.NodeRadius;
    public float InternodeLength => _size.ValueRO.InternodeLength;

    public void Grow(float energy)
    {
        var requestedNodeEnergy = math.min(energy, MaxNodeRadius - NodeRadius);
        _size.ValueRW.NodeRadius += requestedNodeEnergy;

        energy -= requestedNodeEnergy;

        var requestedInternodeEnergy = math.min(energy, MaxInternodeLength - InternodeLength);
        _size.ValueRW.InternodeLength += requestedInternodeEnergy;
        _size.ValueRW.InternodeLength = math.max(_size.ValueRO.InternodeLength, _size.ValueRO.NodeRadius);

        Physics.UpdateSize(InternodeLength - NodeRadius,
                           NodeRadius,
                           _primaryGrowthTarget.ValueRO.Density * Volume(NodeRadius, InternodeLength));

        float Volume(float radius, float length) => 3.14f * radius * radius * ((4f / 3f) * radius + length);
    }
}