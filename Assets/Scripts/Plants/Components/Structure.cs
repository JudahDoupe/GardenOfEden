using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    [Header("Growth Properties")]
    public float AgeInDays = 0;
    public float DaysToSprout = 2;
    public float DaysToFullyGrown = 3;
    public float DaysToDouble = 100;
    public bool IsMature => _hasSprouted && Branches.All(x => x._hasSprouted);

    public Volume Cellulose { get; set; }
    public Volume StarchCapacity => Volume.FromCubicMeters(Cellulose._cubicMeters / 8);
    public Volume StoredStarch { get; set; }
    public Volume WaterCapacity => Volume.FromCubicMeters(Cellulose._cubicMeters / 8);
    public Volume StoredWater => Volume.FromCubicMeters((Plant.StoredWater._cubicMeters / Plant.WaterCapacity._cubicMeters) * WaterCapacity._cubicMeters);

    [Header("Physical Properties")]
    public float Diameter;
    public float Length;
    public List<Connection> Connections;
    public List<Structure> Branches { get; set; } = new List<Structure>();
    public Plant Plant { get; set; }

    private Rigidbody _rigidbody;
    private GameObject _model;
    private int _resourceIndex;
    private bool _hasSprouted = false;
    private bool _isFullyGrown = false;
    private bool _isAlive = true;

    public static Structure Create(Plant plant, int resourceIndex)
    {
        var resource = Resources.Load<GameObject>(plant.Dna.Resources[resourceIndex]);
        var structure = Instantiate(resource).GetComponent<Structure>();

        plant.Structures.Add(structure);
        structure.Plant = plant;
        structure._resourceIndex = resourceIndex;
        structure._model = structure.transform.Find("Model").gameObject;
        structure._rigidbody = structure.gameObject.AddComponent<Rigidbody>();
        structure._rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        structure._isAlive = plant.IsAlive;
        structure.Cellulose = Volume.FromCubicMeters(0.1f);
        structure.UpdateModel();

        return structure;
    }

    public virtual void Grow(float days)
    {
        if (!_isAlive) return;

        //TODO: Use stored starch to increase cellulose

        AgeInDays += days;
        UpdateModel();

        if (AgeInDays > DaysToSprout && !_hasSprouted)
        {
            Sprout();
        }

        if (AgeInDays > DaysToFullyGrown || _isFullyGrown)
        {
            Destroy(_rigidbody);
            _isFullyGrown = true;
        }

        foreach (var branch in Branches)
        {
            branch.Grow(days);
        }
    }

    public void Sprout()
    {
        if (_hasSprouted) return;

        foreach (var connection in Connections)
        {
            var structure = Create(Plant, _resourceIndex + 1);
            structure.transform.parent = connection.transform;
            structure.transform.localPosition = Vector3.zero;
            structure.transform.localRotation = Quaternion.identity;
            Branches.Add(structure);
        }
        _hasSprouted = true;
    }

    public void Kill()
    {
        _isAlive = false;
        foreach(var branch in Branches)
        {
            branch.Kill();
        }
    }

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
}