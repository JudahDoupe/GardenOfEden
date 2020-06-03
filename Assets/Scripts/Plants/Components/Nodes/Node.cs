using System.Collections.Generic;
using UnityEngine;

public class Node : TimeTracker
{
    public Plant Plant { get; set; }
    public Node Base { get; set; }
    public List<Node> Branches { get; set; } = new List<Node>();
    public Internode Internode { get; set; }
    public RenderingInstanceData Mesh { get; set; }

    public bool IsAlive { get; set; } = true;
    public float Size { get; set; }

    public static N Create<N>(Node baseNode, Plant plant = null) where N : Node
    {
        var node = new GameObject(typeof(N).ToString()).AddComponent<N>();

        node.CreationDate = EnvironmentApi.GetDate();
        node.LastUpdateDate = node.CreationDate;

        node.Plant = plant == null ? baseNode.Plant : plant;
        node.Base = baseNode;

        if (baseNode != null) baseNode.Internode = Internode.Create(node, baseNode);

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
        IsAlive = false;

        foreach (var node in Branches)
        {
            node.Kill();
        }

        if (Mesh != null) InstancedMeshRenderer.RemoveInstance(Mesh);
        if (Internode != null) Internode.Kill();

        Destroy(gameObject);
    }

}
