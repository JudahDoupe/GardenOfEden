using Unity.Entities;
using UnityEngine;

public struct NodeRenderer : IComponentData
{
    public Entity NodeRendererEntity;
    public Entity InternodeRendererEntity;
}

public class NodeRendererComponent : MonoBehaviour
{
    public GameObject NodeRenderer;
    public GameObject InternodeRenderer;
}

public class NodeRendererComponentBaker : Baker<NodeRendererComponent>
{
    public override void Bake(NodeRendererComponent authoring)
    {
        AddComponent(new NodeRenderer
        {
            NodeRendererEntity = GetEntity(authoring.NodeRenderer),
            InternodeRendererEntity = GetEntity(authoring.InternodeRenderer)
        });
    }
}