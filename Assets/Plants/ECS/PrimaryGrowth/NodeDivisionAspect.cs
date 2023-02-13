using Unity.Entities;
using Unity.Transforms;

public readonly partial struct NodeDivisionAspect : IAspect
{
    public readonly Entity Entity;
    public readonly TransformAspect Transform;

    private readonly RefRO<Size> _size;
    private readonly RefRO<Dna> _dna;
    private readonly RefRO<Parent> _parent;
    
    private readonly RefRO<NodeDivision> _nodeDivision;
    
    public bool IsReadyToDivide => _size.ValueRO.NodeSize > 0.99f;
    public Entity Structure => _dna.ValueRO.StructurePrefab;
    public Entity Parent => _parent.ValueRO.Value;
}
