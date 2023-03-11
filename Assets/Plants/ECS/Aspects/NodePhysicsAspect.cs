using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

public readonly partial struct NodePhysicsAspect : IAspect
{
    private readonly RefRW<PhysicsConstrainedBodyPair> _bodyPair;
    private readonly RefRW<Spring> _spring;
    private readonly RefRO<Parent> _parent;

    public Entity Node => _parent.ValueRO.Value;
    public Entity BaseNode
    {
        get => _bodyPair.ValueRO.EntityB;
        set => _bodyPair.ValueRW = new PhysicsConstrainedBodyPair(_parent.ValueRO.Value, value, false);
    }
}
