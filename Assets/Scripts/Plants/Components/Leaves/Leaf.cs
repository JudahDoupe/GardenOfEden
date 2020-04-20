using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : TimeTracker
{
    public Node Node;
    public Plant Plant;
    public LeafDna Dna;
    public LeafMesh Mesh;

    public float Size;

    public static Leaf Create(Node node)
    {
        var dna = node.Plant.Dna.LeafDna;
        var leaf = new GameObject(dna.Type.ToString()).AddComponent<Leaf>();

        leaf.transform.parent = node.transform;
        leaf.transform.localPosition = new Vector3(0, 0, 0);
        leaf.transform.localRotation = Quaternion.identity;
        leaf.gameObject.AddComponent<Rigidbody>().isKinematic = true;
        leaf.gameObject.AddComponent<MeshRenderer>().material = dna.Material;
        leaf.Mesh = new LeafMesh(leaf.gameObject.AddComponent<MeshFilter>().mesh, 5);

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

        UpdateMesh();

        return availableSugar;
    }

    public void UpdateMesh()
    {
        var baseStem = Node.BaseNode == null ? null : Node.BaseNode.Stem.Mesh;

        Mesh.Center.Bottom = new Vector3(0, 0, -Size);
        for (int i = 0; i < Mesh.Sides.Length; i++)
        {
            Mesh.Sides[i].Top = Mesh.Sides[i].Direction * Size;
            Mesh.Sides[i].Bottom = Mesh.Sides[i].Top + Mesh.Center.Bottom;
        }

        Mesh.QuickUpdateMesh();
        Node.transform.localPosition = Node.transform.localRotation * Vector3.forward * Size;
    }
}
