using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Plant Plant { get; set; }
    public Node Base { get; set; }
    public List<Node> Branches { get; set; } = new List<Node>();
    public Internode Internode { get; set; }
    public RenderingInstanceData Mesh { get; set; }

    public float CreationDate { get; set; }
    public float LastUpdateDate { get; set; }
    public float Age => EnvironmentApi.GetDate() - CreationDate;
    public NodeType Type;
    public float Size;

    public static Node Create(NodeType type, Node baseNode, Plant plant = null)
    {
        var node = new GameObject(type.ToString()).AddComponent<Node>();

        node.CreationDate = EnvironmentApi.GetDate();
        node.LastUpdateDate = node.CreationDate;

        node.Plant = plant == null ? baseNode.Plant : plant;
        node.Base = baseNode;
        node.Type = type;

        if (type == NodeType.Leaf) node.Mesh = InstancedMeshRenderer.AddInstance("Leaf");
        if (type == NodeType.Flower) node.Mesh = InstancedMeshRenderer.AddInstance("Flower");

        if (baseNode != null)
        {
            baseNode.Branches.Add(node);
            node.Internode = Internode.Create(node, baseNode);
        }

        node.transform.parent = node.Base == null ? node.Plant.transform : node.Base.transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;

        return node;
    }

    public virtual void UpdateMesh()
    {
        if (Mesh != null) Mesh.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(Size, Size, Size));
        if (Internode != null) Internode.UpdateMesh();
    }

    public virtual void Kill()
    {
        foreach (var node in Branches)
        {
            node.Kill();
        }

        if (Mesh != null) InstancedMeshRenderer.RemoveInstance(Mesh);
        if (Internode != null) Internode.Kill();

        Destroy(gameObject);
    }

}

public enum NodeType
{
    Node,
    Bud,
    ApicalBud,
    Leaf,
    Flower,
}
