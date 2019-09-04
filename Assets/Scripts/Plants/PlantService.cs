using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantService : MonoBehaviour
{
    public List<Transform> SpawnLocations;

    [Header("Debug Settings")]
    public bool LogReproductionFailures;
    public bool LogReproductionSuccesses;

    /* API */

    public static Plant PlantSeed(PlantDNA dna, Vector3 worldPosition)
    {
        var plant = new GameObject().AddComponent<Plant>().GetComponent<Plant>();
        plant.transform.position = worldPosition;
        plant.transform.localEulerAngles = new Vector3(-90, UnityEngine.Random.Range(0, 365), 0);

        plant.DNA = dna;
        plant.IsAlive = true;

        plant.Trunk = Structure.Create(plant, dna.Trunk);
        plant.Trunk.transform.parent = plant.transform;
        plant.Trunk.transform.localPosition = Vector3.zero;
        plant.Trunk.transform.localEulerAngles = Vector3.zero;

        return plant;
    }
    public static void SpawnSpecies(PlantDNA dna)
    {
        foreach (var location in Instance.SpawnLocations)
        {
            PlantSeed(dna, location.position);
        }
    }
    public static int GetSpeciesPopulation(Guid speciesId)
    {
        return FindObjectsOfType<Plant>().Where(p => p.GetDNA().SpeciesId == speciesId).Count();
    }

    /* INNER MECHINATIONS */

    public static PlantService Instance;

    void Awake()
    {
        Instance = this;
    }

}
