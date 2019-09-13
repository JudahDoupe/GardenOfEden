using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantService : MonoBehaviour
{
    public List<Transform> SpawnLocations;

    [Header("Debug Settings")]
    public bool LogReproductionFailures;
    public bool LogReproductionSuccesses;

    /* API */

    public static void TryPlantSeed(PlantDNA dna, Vector3 worldPosition)
    {
        Instance._seedQueue.Enqueue(new Tuple<PlantDNA, Vector3>(dna, worldPosition));
    }

    public static void SpawnSpecies(PlantDNA dna)
    {
        foreach (var location in Instance.SpawnLocations)
        {
            Instance.SpawnPlant(dna, location.position);
        }
    }
    public static int GetSpeciesPopulation(Guid speciesId)
    {
        return FindObjectsOfType<Plant>().Count(p => p.GetDNA().SpeciesId == speciesId);
    }

    /* INNER MECHINATIONS */

    public static PlantService Instance;

    private readonly Queue<Tuple<PlantDNA, Vector3>> _seedQueue = new Queue<Tuple<PlantDNA, Vector3>>();
    private bool _isQueueBeingProcessed;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (_seedQueue.Any() && !_isQueueBeingProcessed)
        {
            StartCoroutine(ProcessSeedQueue());
        }
    }

    private IEnumerator ProcessSeedQueue()
    {
        _isQueueBeingProcessed = true;
        while (_seedQueue.Any())
        {
            yield return new WaitForSeconds(0.1f);
            var (dna, seedLocation) = _seedQueue.Dequeue();
            var plantLocation = GetPlantableLocation(dna, seedLocation);
            if (plantLocation.HasValue)
            {
                SpawnPlant(dna, plantLocation.Value);
            }
        }
        _isQueueBeingProcessed = false;
    }

    private Vector3? GetPlantableLocation(PlantDNA dna, Vector3 worldPosition)
    {
        var ray = new Ray(worldPosition + Vector3.up * 10, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 50))
        {
            if (EnvironmentService.GetSoil(hit.point) > 0)
            {
                if (Instance.LogReproductionFailures)
                {
                    Debug.Log($"No Suitable soil was found to plant {dna.Name ?? "your plant"}.");
                }
            }
            else
            {
                if (Instance.LogReproductionSuccesses)
                {
                    Debug.Log($"Successfully planted {GetSpeciesPopulation(dna.SpeciesId)}th {dna.Name ?? "your plant"}.");
                }

                return hit.point;
            }
        }
        else
        {
            if (Instance.LogReproductionFailures)
            {
                Debug.Log($"There was no terrain to plant {dna.Name ?? "your plant"}.");
            }
        }

        return null;
    }

    private Plant SpawnPlant(PlantDNA dna, Vector3 worldPosition)
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

}
