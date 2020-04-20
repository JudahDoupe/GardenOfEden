using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stem : TimeTracker
{
    public Node Node;
    public Plant Plant;
    public StemDna Dna;
    public StemMesh Mesh;

    public float Length;
    public float Radius;

    public static Stem Create(Node node)
    {
        var dna = node.Plant.Dna.StemDna;
        var stem = new GameObject(dna.Type.ToString()).AddComponent<Stem>();

        stem.transform.parent = node.transform;
        stem.transform.localPosition = new Vector3(0, 0, 0);
        stem.transform.localRotation = Quaternion.identity;
        stem.gameObject.AddComponent<Rigidbody>().isKinematic = true;
        stem.gameObject.AddComponent<MeshRenderer>().material = dna.Material;
        stem.Mesh = new StemMesh(stem.gameObject.AddComponent<MeshFilter>().mesh, 5);

        stem.Node = node;
        stem.Plant = node.Plant;
        stem.Dna = dna;

        stem.CreationDate = node.CreationDate;
        stem.LastUpdateDate = node.LastUpdateDate;

        return stem;
    }

    void OnTriggerEnter(Collider collider)
    {
        Plant plant = collider.transform.GetComponentInParent<Plant>();

        if (plant != Node.Plant)
        {
            Node.Kill();
        }
    }

    public Volume Grow(Volume availableSugar) //TODO: use sugar
    {
        LastUpdateDate = EnvironmentApi.GetDate();

        var percentGrown = Age / Dna.DaysToMaturity;
        var primaryGrowth = Mathf.Pow(percentGrown, 2);
        var secondaryGrowth = Mathf.Pow(percentGrown, 1.2f) / percentGrown;
        var growth = Mathf.Lerp(primaryGrowth, secondaryGrowth, percentGrown);
        growth = float.IsNaN(growth) ? 0 : growth;

        Length = Dna.PrimaryLength * growth;
        Radius = Dna.PrimaryRadius * growth;

        UpdateMesh();

        return availableSugar;
    }

    public void UpdateMesh()
    {
        var baseStem = Node.BaseNode == null ? null : Node.BaseNode.Stem.Mesh;

        Mesh.Center.Bottom = new Vector3(0, 0, -Length);
        for (int i = 0; i < Mesh.Sides.Length; i++)
        {
            Mesh.Sides[i].Top = Mesh.Sides[i].Direction * Radius;
            Mesh.Sides[i].Bottom = (baseStem ?? Mesh).Sides[i].Top + Mesh.Center.Bottom;
        }

        Mesh.QuickUpdateMesh();
        Node.transform.localPosition = Node.transform.localRotation * Vector3.forward * Length;
    }
}