using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

public class Structure : MonoBehaviour
{
    private const float DaysToMaturity = 1;
    private const float DaysToDouble = 50;

    public float DaysOld;
    public Plant Plant;
    public PlantDNA.Structure DNA;
    public Connection BaseConnection { get; set; }
    public List<Connection> Connections { get; set; } = new List<Connection>();

    public GameObject Model { get; set; }
    private Rigidbody _rigidbody;
    private bool _hasSprouted = false;
    private bool _isAlive = true;

    public static Structure Create(Plant plant, PlantDNA.Structure dna)
    {
        var structure = Instantiate(dna.Prefab).GetComponent<Structure>();
        if (structure == null)
            Debug.Log("You forgot to add a SelectedStructure component to your prefab DUMBASS!!!");

        structure.transform.localPosition = Vector3.zero;
        structure.DaysOld = plant.IsAlive ? 0 : DaysToMaturity;
        structure.Plant = plant;
        structure.DNA = new PlantDNA.Structure
        {
            Connections = dna.Connections,
            Girth = dna.Girth,
            Length = dna.Length,
            Prefab = dna.Prefab,
            Type = dna.Type
        };

        structure.Model = structure.transform.Find("Model").gameObject;
        structure._rigidbody = structure.gameObject.AddComponent<Rigidbody>();
        structure._rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        structure._isAlive = plant.IsAlive;

        structure.UpdateModel();
        structure.StartCoroutine(structure.Grow());

        return structure;
    }

    public Connection Connect(Structure structure, Vector3 localPosition)
    {
        var position = Vector3.Scale(localPosition, new Vector3(0, 0, 1));
        var rotation = Quaternion.LookRotation(localPosition.normalized, Vector3.up);
        var connection = Connection.Create(this, structure, position, rotation);
        return connection;
    }

    public IEnumerator Grow()
    {
        var startTime = Time.time;
        while (_isAlive)
        {
            _isAlive = Plant.IsAlive;
            DaysOld += (Time.time - startTime) / 3f;
            startTime = Time.time;

            UpdateModel();

            if (DaysOld < DaysToMaturity)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                if (!_hasSprouted)
                {
                    foreach (var connection in DNA.Connections)
                    {
                        Connection.Create(this, connection);
                    }

                    _hasSprouted = true;
                }
                yield return new WaitForSeconds(10);
            }
        }
    }

    public void UpdateModel()
    {
        var primaryGrowth = 1 / (1 + Mathf.Exp(5 - 10 / DaysToMaturity * DaysOld));
        var secondaryGrowth = 1 + DaysOld / DaysToDouble;

        transform.localScale = new Vector3(primaryGrowth, primaryGrowth, primaryGrowth);
        Model.transform.localScale = new Vector3(DNA.Girth * secondaryGrowth, DNA.Girth * secondaryGrowth, DNA.Length);
    }

    void OnCollisionEnter(Collision collision)
    {
        Plant plant = collision.collider.transform.ParentWithComponent<Plant>()?.GetComponent<Plant>();

        if (plant != null && plant != Plant)
        {
            _isAlive = false;
        }
    }

    public PlantDNA.Structure GetDNA()
    {
        return new PlantDNA.Structure
        {
            Prefab = DNA.Prefab,
            Length = DNA.Length,
            Girth = DNA.Girth,
            Connections = Connections.Select(c => c.GetDNA()).ToList()
        };
    }
}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}