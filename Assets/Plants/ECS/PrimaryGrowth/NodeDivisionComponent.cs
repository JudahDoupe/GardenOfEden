using Unity.Entities;
using UnityEngine;

public struct NodeDivision : IComponentData { }

public class NodeDivisionComponent : MonoBehaviour { }

public class NodeDivisionComponentBaker : Baker<NodeDivisionComponent>
{
    public override void Bake(NodeDivisionComponent authoring)
    {
        AddComponent(new NodeDivision());
    }
}