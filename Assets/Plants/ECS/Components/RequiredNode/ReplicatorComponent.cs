using Unity.Entities;
using UnityEngine;

public struct Replicator : IComponentData { }

public class ReplicatorComponent : MonoBehaviour { }

public class ReplicatorComponentBaker : Baker<ReplicatorComponent>
{
    public override void Bake(ReplicatorComponent authoring)
    {
        AddComponent(new Replicator()); 
    }
}