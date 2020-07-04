using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

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
    public PlantDna.NodeDna Dna => Plant.PlantDna.Nodes.FirstOrDefault(x => x.Type == Type) ?? new PlantDna.NodeDna();
    public string Type = "Node";
    public float Size;


    public Node AddNodeAfter()
    {
        var node = Create(Plant);
        Branches.Add(node);
        node.Base = this;
        node.transform.parent = transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;
        return node;
    }
    public Node AddNodeBefore()
    {
        var node = Create(Plant);
        if (Base != null)
        {
            Base.Branches.Remove(this);
            Base.Branches.Add(node);
            node.transform.parent = Base.transform;
            node.Base = Base;
        }

        Base = node;
        transform.parent = node.transform;
        node.Branches.Add(this);

        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;
        return node;
    }
    private Node Create(Plant plant)
    {
        var node = new GameObject("Node").AddComponent<Node>();
        node.CreationDate = EnvironmentApi.GetDate();
        node.LastUpdateDate = node.CreationDate;
        node.Plant = plant;
        return node;
    }

    public void Kill()
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
