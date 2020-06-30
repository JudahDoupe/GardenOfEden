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
    public PlantDna.NodeType Type;
    public PlantDna.Node Dna;
    public float Size;

    public static Node Create(PlantDna.NodeType type, Node baseNode, Plant plant = null)
    {
        var node = new GameObject(type.ToString()).AddComponent<Node>();

        node.CreationDate = EnvironmentApi.GetDate();
        node.LastUpdateDate = node.CreationDate;

        node.Plant = plant == null ? baseNode.Plant : plant;
        node.Dna = node.Plant.Dna.GetNodeDna(type);
        node.Base = baseNode;
        node.Type = type;

        if (!string.IsNullOrWhiteSpace(node.Dna.MeshId)) node.Mesh = InstancedMeshRenderer.AddInstance(node.Dna.MeshId);

        if (baseNode != null)
        {
            baseNode.Branches.Add(node);
            if (node.Dna.Internode != null && node.Dna.Internode.Length > 0.001f)
            {
                node.Internode = Internode.Create(node, baseNode);
            }
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

        if (Base != null) Base.Branches.Remove(this);
        if (Mesh != null) InstancedMeshRenderer.RemoveInstance(Mesh);
        if (Internode != null) Internode.Kill();

        Destroy(gameObject);
    }

}
