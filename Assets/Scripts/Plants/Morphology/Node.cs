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
        var newBase = Create(Plant);
        var oldBase = Base;
        if (oldBase != null)
        {
            oldBase.Branches.Remove(this);
            newBase.Base = oldBase;
            newBase.transform.parent = oldBase.transform;
            oldBase.Branches.Add(newBase);
        }
        Base = newBase;
        transform.parent = newBase.transform;
        newBase.Branches.Add(this);

        newBase.transform.position = transform.position;
        newBase.transform.rotation = transform.rotation;
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.identity;
        return newBase;
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
