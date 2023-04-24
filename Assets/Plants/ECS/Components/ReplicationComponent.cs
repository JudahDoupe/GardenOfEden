using Unity.Entities;
using UnityEngine;

public struct Replication : IComponentData { }

public class ReplicationComponent : MonoBehaviour { }

public class ReplicationComponentBaker : Baker<ReplicationComponent>
{
    public override void Bake(ReplicationComponent authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(e, new Replication());
    }
}