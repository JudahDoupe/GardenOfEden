using System.Collections.Generic;
using System.Collections;
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
        node.Base = baseNode;
        node.SetType(type);

        if (!string.IsNullOrWhiteSpace(node.Dna.MeshId)) node.Mesh = InstancedMeshRenderer.AddInstance(node.Dna.MeshId);

        if (baseNode != null)
        {
            baseNode.Branches.Add(node);
        }

        node.transform.parent = node.Base == null ? node.Plant.transform : node.Base.transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;

        return node;
    }

    public void SetType(PlantDna.NodeType type)
    {
        Type = type;
        Dna = Plant.Dna.GetNodeDna(type);
        if (Base != null && Dna.Internode != null && Dna.Internode.Length > 0.001f)
        {
            Internode = Internode.Create(this, Base);
        }
    }

    public void UpdateMesh()
    {
        if (Mesh != null) Mesh.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(Size, Size, Size));
        if (Internode != null) Internode.UpdateMesh();
    }
    public IEnumerator SmoothUpdateMesh(float seconds)
    {
        if (Mesh == null) yield break;
        if (Internode != null) StartCoroutine(Internode.SmoothUpdateMesh(seconds));

        var oldPosition = Mesh.Matrix.GetColumn(3);
        var oldRotation = Quaternion.LookRotation(
            Mesh.Matrix.GetColumn(2),
            Mesh.Matrix.GetColumn(1)
        );
        var oldScale = new Vector3(
            Mesh.Matrix.GetColumn(0).magnitude,
            Mesh.Matrix.GetColumn(1).magnitude,
            Mesh.Matrix.GetColumn(2).magnitude
        );
        var newScale = new Vector3(Size, Size, Size);

        var t = 0f;
        while (t < seconds)
        {
            Mesh.Matrix = Matrix4x4.TRS(Vector3.Lerp(oldPosition, transform.position, t / seconds), 
                                        Quaternion.Lerp(oldRotation, transform.rotation, t / seconds),
                                        Vector3.Lerp(oldScale, newScale, t / seconds));
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
        
        Mesh.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, newScale);
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
