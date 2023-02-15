using Unity.Entities;
using Unity.Transforms;

public readonly partial struct ReplicationAspect : IAspect
{
    public readonly Entity Entity;
    public readonly TransformAspect Transform;

    private readonly RefRO<Size> _size;
    private readonly RefRO<Dna> _dna;
    private readonly RefRO<Parent> _parent;
    
    private readonly RefRO<Replicator> _nodeDivision;
    
    public bool IsReadyToDivide => _size.ValueRO.NodeRadius > 0.99f;
    public Entity SupportStructure => _dna.ValueRO.SupportStructurePrefab;
    public Entity Parent => _parent.ValueRO.Value;
    public Dna Dna => _dna.ValueRO;
}
