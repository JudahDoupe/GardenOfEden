using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : TimeTracker
{
    public Node Node;
    public Plant Plant;
    public LeafDna Dna;
    public RenderingInstanceData Mesh;

    public float Size;

    public static Leaf Create(Node node)
    {
        var dna = node.Plant.Dna.LeafDna;
        var leaf = new GameObject(dna.Type.ToString()).AddComponent<Leaf>();

        leaf.transform.parent = node.transform;
        leaf.transform.localPosition = new Vector3(0, 0, 0);
        leaf.transform.localRotation = Quaternion.identity;
        leaf.gameObject.AddComponent<Rigidbody>().isKinematic = true;
        leaf.Mesh = InstancedMeshRenderer.AddInstance("Leaf");

        leaf.Node = node;
        leaf.Plant = node.Plant;
        leaf.Dna = dna;

        leaf.CreationDate = node.CreationDate;
        leaf.LastUpdateDate = node.LastUpdateDate;

        return leaf;
    }

    public Volume Grow(Volume availableSugar) //TODO: use sugar
    {
        LastUpdateDate = EnvironmentApi.GetDate();

        var percentGrown = Mathf.Min(Age / Dna.DaysToMaturity, 1);
        var primaryGrowth = Mathf.Pow(percentGrown, 2);
        var growth = primaryGrowth * percentGrown;
        growth = float.IsNaN(growth) ? 0 : growth;

        Size = growth * Dna.Size;
        Mesh.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(Size, Size, Size));

        return availableSugar;
    }

    public void Kill()
    {
        Node.Leaves.Remove(this);
        InstancedMeshRenderer.RemoveInstance(Mesh);
        Destroy(gameObject);
    }
}
