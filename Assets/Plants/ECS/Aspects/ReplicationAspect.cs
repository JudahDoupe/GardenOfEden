using Unity.Entities;
using Unity.Transforms;

public readonly partial struct ReplicationAspect : IAspect
{

    private readonly RefRO<Dna> _dna;
    private readonly RefRO<Parent> _parent;
    private readonly RefRO<Replication> _replication;
    private readonly RefRO<LocalTransform> _transform;

    public readonly NodeAspect Node;
    
    public bool IsReadyToDivide => Node.IsMature;
    public LocalTransform LocalTransform => _transform.ValueRO;
    public Entity SupportStructure => _dna.ValueRO.SupportStructurePrefab;
    public Entity Parent => _parent.ValueRO.Value;
    public Dna Dna => _dna.ValueRO;
}
