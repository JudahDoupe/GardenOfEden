using Unity.Entities;
using UnityEngine;

public struct Node : IComponentData
{
    public Entity NodeRendererEntity;
    public Entity InternodeRendererEntity;
}

public class NodeComponent : MonoBehaviour
{
    public GameObject NodeRenderer;
    public GameObject InternodeRenderer;
}

public class NodeComponentBaker : Baker<NodeComponent>
{
    public override void Bake(NodeComponent authoring)
    {
        AddComponent(new Node
        {
            NodeRendererEntity = GetEntity(authoring.NodeRenderer),
            InternodeRendererEntity = GetEntity(authoring.InternodeRenderer)
        });
    }
}