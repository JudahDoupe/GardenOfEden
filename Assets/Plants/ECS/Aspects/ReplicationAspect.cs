using Unity.Entities;
using Unity.Transforms;

public readonly partial struct ReplicationAspect : IAspect
{
    private readonly RefRO<Replicator> _nodeDivision;
    private readonly RefRO<Dna> _dna;

    public readonly PrimaryGrowthAspect PrimaryGrowth;
    public readonly TransformAspect Transform;
    
    public bool IsReadyToDivide => PrimaryGrowth.IsMature;
    public Entity SupportStructure => _dna.ValueRO.SupportStructurePrefab;
    public Dna Dna => _dna.ValueRO;
}
