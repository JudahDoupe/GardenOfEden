using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    private const float DaysToMaturity = 1;
    private const float DaysToDouble = 100;
    public float AgeInDays;

    public Plant Plant;
    public PlantDNA.Structure DNA;
    public Connection BaseConnection { get; set; }
    public List<Connection> Connections { get; set; } = new List<Connection>();

    public bool IsFullyGrown => _hasSprouted && Connections.All(c => c.To.IsFullyGrown);

    public GameObject Model { get; set; }
    private Rigidbody _rigidbody;
    private bool _hasSprouted = false;
    private bool _isAlive = true;

    public static Structure Create(Plant plant, PlantDNA.Structure dna)
    {
        var structure = Instantiate(Resources.Load<GameObject>(dna.Resource).GetComponent<Structure>());
        if (structure == null)
            Debug.LogError("You forgot to add a SelectedStructure component to your prefab DUMBASS!!!");

        structure.transform.localPosition = Vector3.zero;
        structure.Plant = plant;
        structure.DNA = dna;

        structure.Model = structure.transform.Find("Model").gameObject;
        structure._rigidbody = structure.gameObject.AddComponent<Rigidbody>();
        structure._rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        structure._isAlive = plant.IsAlive;
        structure.AgeInDays = plant.IsAlive ? 0 : DaysToMaturity;
        structure.UpdateModel();

        return structure;
    }

    public Connection Connect(Structure structure, Vector3 localPosition)
    {
        var position = Vector3.Scale(localPosition, new Vector3(0, 0, 1));
        var rotation = Quaternion.LookRotation(localPosition.normalized, Vector3.up);
        var connection = Connection.Create(this, structure, position, rotation);
        return connection;
    }

    public void Grow(float days)
    {
        if(!_isAlive) return;

        AgeInDays += days;

        UpdateModel();

        if (AgeInDays > DaysToMaturity && !_hasSprouted)
        {
            foreach (var connection in DNA.Connections)
            {
                Connection.Create(this, connection);
            }

            _hasSprouted = true;
        }

        foreach (var connection in Connections)
        {
            connection.To.Grow(days);
        }
    }

    public void UpdateModel()
    {
        var primaryGrowth = 1 / (1 + Mathf.Exp(5 - 10 / DaysToMaturity * AgeInDays));
        var secondaryGrowth = 1 + AgeInDays / DaysToDouble;

        transform.localScale = new Vector3(primaryGrowth, primaryGrowth, primaryGrowth);
        Model.transform.localScale = new Vector3(DNA.Diameter * secondaryGrowth, DNA.Diameter * secondaryGrowth, DNA.Length);
    }

    public int GetRecursiveDepth()
    {
        return BaseConnection?.From == null ? 1 : BaseConnection.From.GetRecursiveDepth();
    }

    void OnCollisionEnter(Collision collision)
    {
        Plant plant = collision.collider.transform.ParentWithComponent<Plant>()?.GetComponent<Plant>();

        if (plant != null && plant != Plant)
        {
            _isAlive = false;
        }
    }

    public PlantDNA.Structure GenerateDNA()
    {
        return new PlantDNA.Structure
        {
            Type = DNA.Type,
            Resource = DNA.Resource,
            Length = DNA.Length,
            Diameter = DNA.Diameter,
            Connections = Connections.Select(c => c.GenerateDNA()).ToList()
        };
    }
}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}