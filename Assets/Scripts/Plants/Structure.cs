using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

public class Structure : MonoBehaviour
{
    private const float SecondaryGrowthSpeed = 20;
    private const float DaysToMaturity = 1;

    public float DaysOld;
    public Plant Plant;
    public PlantDNA.Structure DNA;
    public Connection BaseConnection { get; set; }
    public List<Connection> Connections { get; set; } = new List<Connection>();

    private GameObject _model;
    private Rigidbody _rigidbody;
    private bool _hasSprouted = false;
    private bool _isAlive = true;

    public static Structure Create(Plant plant, PlantDNA.Structure dna)
    {
        var structure = Instantiate(dna.Prefab).GetComponent<Structure>();
        if (structure == null)
            Debug.Log("You forgot to add a Structure component to your prefab DUMBASS!!!");

        structure.transform.localPosition = Vector3.zero;
        structure.DaysOld = plant.IsAlive ? 0 : DaysToMaturity;
        structure.Plant = plant;
        structure.DNA = dna;

        structure._model = structure.transform.Find("Model").gameObject;
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
        Connections.Add(connection);
        return connection;
    }

    public IEnumerator Grow()
    {
        var startTime = Time.time;
        var deltaTime = 0f; 
        while (_isAlive)
        {
            _isAlive = Plant.IsAlive;
            DaysOld += (deltaTime) / 3f;

            UpdateModel();

            if (!_hasSprouted)
            {
                foreach (var connection in DNA.Connections)
                {
                    Connection.Create(this, connection);
                }

                _hasSprouted = true;
            }

            if (DaysOld > DaysToMaturity)
            {
                yield return new WaitForSeconds(10);
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }

            deltaTime = Time.time - startTime;
            startTime = Time.time;
        }
    }

    public void UpdateModel()
    {
        var primaryGrowth = 1 / (1 + Mathf.Exp(5 - 10 / DaysToMaturity * DaysOld));
        var secondaryGrowth = 1 + DaysOld / SecondaryGrowthSpeed;

        transform.localScale = new Vector3(primaryGrowth, primaryGrowth, primaryGrowth);
        _model.transform.localScale = new Vector3(DNA.Girth * secondaryGrowth, DNA.Girth * secondaryGrowth, DNA.Length);
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

    public void Clicked(Vector3 hitPosition)
    {
        var pedestal = transform.ParentWithComponent<PlantCreationPedestal>()?.GetComponent<PlantCreationPedestal>();
        if (pedestal != null && pedestal.SelectedDna != null)
        {
            var structure = Create(Plant, pedestal.SelectedDna);
            Connect(structure, transform.InverseTransformPoint(hitPosition));
        }
    }
}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}