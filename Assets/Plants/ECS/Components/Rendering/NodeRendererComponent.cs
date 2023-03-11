using Unity.Entities;
using UnityEngine;

public struct NodeRenderer : IComponentData
{
    public Entity Node;
}

public class NodeRendererComponent : MonoBehaviour
{
    public GameObject Node;
}

public class NodeRendererComponentBaker : Baker<NodeRendererComponent>
{
    public override void Bake(NodeRendererComponent authoring)
    {
        AddComponent(new NodeRenderer
        {
            Node = GetEntity(authoring.Node)
        });
    }
}