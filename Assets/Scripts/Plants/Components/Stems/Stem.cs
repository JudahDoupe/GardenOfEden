using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stem : TimeTracker
{
    public Node Node;
    public Plant Plant;
    public StemDna Dna;

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

    public Volume Grow(Volume availableSugar)
    {
        LastUpdateDate = EnvironmentApi.GetDate();
        return availableSugar;
    }

    /*
    public void UpdateModel()
    {
        //TODO: use volumes to determine length and radius 
        var primaryGrowth = 1 / (1 + Mathf.Exp(5 - 10 / DaysToSprout * AgeInDays));
        var secondaryGrowth = 1 + AgeInDays / DaysToDouble;

        transform.localScale = new Vector3(primaryGrowth, primaryGrowth, primaryGrowth);
        var modelScale = new Vector3(Diameter * secondaryGrowth, Diameter * secondaryGrowth, Length);
        _model.transform.localScale = modelScale;

        Cellulose = Volume.FromCubicMeters(Length * Mathf.PI * Mathf.Pow(Diameter / 2f, 2));

        foreach (var connection in Connections)
        {
            connection.UpdatePosition(_model.transform.localScale);
        }
    }
    */
}
