using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ReproductionService : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool LogReproductionFailures;
    public bool LogReproductionSuccesses;

    /* Publicly Accessible Methods */

    public void DropSeed(PlantDna dna, Vector3 location)
    {
        _seedQueue.Enqueue(new Tuple<PlantDna, Vector3>(dna, location));
    }

    public Plant PlantSeed(PlantDna dna, Vector3 location)
    {
        var plant = new GameObject().AddComponent<Plant>();
        plant.PlantId = lastPlantId++;
        plant.transform.position = location;
        plant.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);
        plant.PlantDna = dna;

        if (LogReproductionSuccesses)
        {
            Debug.Log($"Successfully planted {PlantApi.GetSpeciesPopulation(dna.SpeciesId)}th {dna.Name ?? "your plant"}.");
        }

        return plant;
    }

    /* Inner Mechinations */

    private LandService _landService;

    private int lastPlantId = 10;
    private Queue<Tuple<PlantDna, Vector3>> _seedQueue = new Queue<Tuple<PlantDna, Vector3>>();

    void Start()
    {
        _landService = GetComponent<LandService>();
    }
    void Update()
    {
        if (_seedQueue.Any())
        {
            DropNextSeed();
        }
    }

    private void DropNextSeed()
    {
        var seed = _seedQueue.Dequeue();
        if (IsLocationFertile(seed.Item2))
        {
            PlantSeed(seed.Item1, DI.LandService.ClampAboveTerrain(seed.Item2));
        }
    }

    private bool IsLocationFertile(Vector3 worldPosition)
    {
        var landHeight = _landService.SampleTerrainHeight(worldPosition);
        worldPosition.y = landHeight;
        var waterDepth = _landService.SampleWaterDepth(worldPosition);
        var soilDepth = _landService.SampleSoilDepth(worldPosition);
        var rootDepth = _landService.SampleRootDepth(worldPosition);

        if (soilDepth < 0.05f)
            DebugLackOfResources("soil");
        else if (rootDepth > 0.01f)
            DebugLackOfResources("root space");
        else if (waterDepth < 0.1f)
            DebugLackOfResources("water");
        else
            return true;

        return false;
    }

    private void DebugLackOfResources(string resource)
    {
        if (LogReproductionFailures)
        {
            Debug.Log($"Not enough {resource} to plant.");
        }
    }

}
