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
        leaf.Mesh = new LeafMesh(leaf.gameObject.AddComponent<MeshFilter>().mesh, 8, leaf.L, leaf.W, leaf.H);

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
        foreach(var edge in Mesh.Edges)
        {
            edge.Bottom = edge.Vector * Size;
            edge.Top = edge.Bottom;
        }

        Mesh.QuickUpdateMesh();
    }

    public float Length = 1;
    public float Curl = 0.1f;
    public float Width => 1/Length;

    protected float L(float theta)
    {
        var e = 1.2f;
        if (theta < Mathf.PI)
        {
            return Mathf.Pow(theta, e) * Length / 4;
        }
        else
        {
            var f = Mathf.Abs(theta - 2 * Mathf.PI);
            return Mathf.Pow(f, e) * Length / 4;
        }
    }
    protected float W(float theta)
    {
        return Mathf.Sin(theta) * Width / 4;
    }
    protected float H(float theta)
    {
        var pi = Mathf.PI / 2;
        var pi2 = Mathf.Pow(pi, 2);
        var offset = theta < Mathf.PI ? 1 : 3;
        return (Mathf.Pow(theta - offset * pi, 2) - pi2) / pi2 * Curl;
    }
}
