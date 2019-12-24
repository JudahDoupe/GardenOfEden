using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public float Diameter;
    public float Length;

    public float AgeInDays = 0;
    public float DaysToSprout = 2;
    public float DaysToFullyGrown = 3;
    public float DaysToDouble = 100;

    public bool IsMature => _hasSprouted && Branches.All(x => x._hasSprouted);

    public Plant Plant;
    public List<Transform> Connections;
    public List<Structure> Branches { get; set; } = new List<Structure>();

    private GameObject _model;
    private Rigidbody _rigidbody;
    private int _resourceIndex;
    private bool _hasSprouted = false;
    private bool _isFullyGrown = false;
    private bool _isAlive = true;

    public static Structure Create(Plant plant, int resourceIndex)
    {
        var resource = Resources.Load<GameObject>(plant.Dna.Resources[resourceIndex]);
        var structure = Instantiate(resource).GetComponent<Structure>();

        structure.Plant = plant;
        structure._resourceIndex = resourceIndex;
        structure._model = structure.transform.Find("Model").gameObject;
        structure._rigidbody = structure.gameObject.AddComponent<Rigidbody>();
        structure._rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        structure._isAlive = plant.IsAlive;
        structure.UpdateModel();

        foreach (var renderer in structure.GetComponentsInChildren<Renderer>())
        {
            renderer.material.SetFloat("_LightAbsorptionId", plant.PlantId + 0.5f);
        }

        return structure;
    }

    public void Grow(float days)
    {
        if (!_isAlive) return;

        AgeInDays += days;
        UpdateModel();

        if (AgeInDays > DaysToSprout && !_hasSprouted)
        {
            Srpout();
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

    public void Srpout()
    {
        if (_hasSprouted) return;

        foreach (var connection in Connections)
        {
            var structure = Create(Plant, _resourceIndex + 1);
            structure.transform.parent = connection;
            structure.transform.localPosition = Vector3.zero;
            structure.transform.localRotation = Quaternion.identity;
            Branches.Add(structure);
        }
        _hasSprouted = true;
    }

    public void UpdateModel()
    {
        //TODO: Move this logic into the state machines
        var primaryGrowth = 1 / (1 + Mathf.Exp(5 - 10 / DaysToSprout * AgeInDays));
        var secondaryGrowth = 1 + AgeInDays / DaysToDouble;

        transform.localScale = new Vector3(primaryGrowth, primaryGrowth, primaryGrowth);
        _model.transform.localScale = new Vector3(Diameter * secondaryGrowth, Diameter * secondaryGrowth, Length);
    }

    void OnCollisionEnter(Collision collision)
    {
        Plant plant = collision.collider.transform.ParentWithComponent<Plant>()?.GetComponent<Plant>();

        if (plant != null && plant != Plant)
        {
            _isAlive = false;
        }
    }
}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}