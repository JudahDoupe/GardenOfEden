﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantApi : MonoBehaviour
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
        return FindObjectsOfType<Plant>().Count(p => p.DNA.SpeciesId == speciesId);
    }
    public static int GetPlantPopulation()
    {
        return FindObjectsOfType<Plant>().Count();
    }

    /* INNER MECHINATIONS */

    public static PlantApi Instance;
    private static RootService _rootService;

    private readonly Queue<Tuple<PlantDNA, Vector3>> _seedQueue = new Queue<Tuple<PlantDNA, Vector3>>();
    private bool _isSeedQueueBeingProcessed;

    private readonly Queue<Plant> _plantUpdateQueue = new Queue<Plant>();
    private bool _isPlantUpdateQueueBeingProcessed;
    private int lastPlantId = 1;

    void Awake()
    {
        Instance = this;
        _rootService = GetComponent<RootService>();
    }

    void Update()
    {
        if (_seedQueue.Any() && !_isSeedQueueBeingProcessed)
        {
            StartCoroutine(ProcessSeedQueue());
        }

        if (_plantUpdateQueue.Any() && !_isPlantUpdateQueueBeingProcessed)
        {
            StartCoroutine(ProcessPlantUpdateQueue());
        }
    }

    private IEnumerator ProcessPlantUpdateQueue()
    {
        _isPlantUpdateQueueBeingProcessed = true;
        while (_plantUpdateQueue.Any())
        {
            yield return new WaitForSeconds(1f);
            var plant = _plantUpdateQueue.Dequeue();
            var growthInDays = EnvironmentApi.GetDate() - plant.LastUpdatedDate;
            plant.Grow(growthInDays);
            _rootService.SpreadRoots(plant, plant.DNA.RootRadius, growthInDays);
            plant.StoredWater += _rootService.AbsorbWater(plant);
            _plantUpdateQueue.Enqueue(plant);
        }
        _isPlantUpdateQueueBeingProcessed = false;
    }
    private IEnumerator ProcessSeedQueue()
    {
        _isSeedQueueBeingProcessed = true;
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
        _isSeedQueueBeingProcessed = false;
    }

    private Vector3? GetPlantableLocation(PlantDNA dna, Vector3 worldPosition)
    {
        var ray = new Ray(worldPosition + Vector3.up * 10, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 50))
        {
            var result = Physics.OverlapSphere(hit.point, dna.RootRadius);
            var waterDepth = EnvironmentApi.SampleWaterDepth(hit.point);
            var soilDepth = EnvironmentApi.SampleSoilDepth(hit.point);
            var rootDepth = _rootService.SampleRootDepth(hit.point);

            if (soilDepth < 0.05f)
            {
                DebugLackOfReources("soil", dna.Name);
            }
            else if (rootDepth > 0.01f)
            {
                DebugLackOfReources("root space", dna.Name);
            }
            else if (waterDepth < 0.1f)
            {
                DebugLackOfReources("water", dna.Name);
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
        var plant = new GameObject().AddComponent<Plant>();
        plant.Id = lastPlantId++;
        plant.transform.position = worldPosition;
        plant.transform.localEulerAngles = new Vector3(-90, UnityEngine.Random.Range(0, 365), 0);

        plant.DNA = dna;
        plant.IsAlive = true;
        plant.PlantedDate = EnvironmentApi.GetDate();
        plant.LastUpdatedDate = plant.PlantedDate;

        plant.Trunk = Structure.Create(plant, dna.Trunk);
        plant.Trunk.transform.parent = plant.transform;
        plant.Trunk.transform.localPosition = Vector3.zero;
        plant.Trunk.transform.localEulerAngles = Vector3.zero;

        _plantUpdateQueue.Enqueue(plant);

        return plant;
    }

    // Helper Methods

    private void DebugLackOfReources(string resource, string plantName)
    {
        if (Instance.LogReproductionFailures)
        {
            Debug.Log($"Not enough {resource} to plant {plantName ?? "your plant"}.");
        }
    }
}
