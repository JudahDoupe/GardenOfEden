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
        var e = GetEntity(TransformUsageFlags.NonUniformScale);
        AddComponent(e, new NodeRenderer
        {
            Node = GetEntity(authoring.Node, TransformUsageFlags.Dynamic)
        });
    }
}