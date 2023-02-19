using Unity.Entities;
using Unity.Transforms;

public readonly partial struct ReplicationAspect : IAspect
{

    private readonly RefRO<Dna> _dna;
    private readonly RefRO<Parent> _parent;
    
    private readonly RefRO<Replicator> _nodeDivision;

    public readonly GrowthAspect Growth;
    public LocalTransform LocalTransform => Growth.Transform.LocalTransform;
    
    public bool IsReadyToDivide => Growth.IsMature;
    public Entity SupportStructure => _dna.ValueRO.SupportStructurePrefab;
    public Entity Parent => _parent.ValueRO.Value;
    public Dna Dna => _dna.ValueRO;
}
