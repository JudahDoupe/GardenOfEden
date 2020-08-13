using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ReproductionService : MonoBehaviour, IDailyProcess
{
    public float UpdateMilliseconds = 1;

    [Header("Debug Settings")]
    public bool LogReproductionFailures;
    public bool LogReproductionSuccesses;

    private int _lastPlantId = 10;
    private Queue<Tuple<PlantDna, Vector3>> _seedQueue = new Queue<Tuple<PlantDna, Vector3>>();

    public void DropSeed(PlantDna dna, Vector3 location)
    {
        _seedQueue.Enqueue(new Tuple<PlantDna, Vector3>(dna, location));
    }

    public Plant PlantSeed(PlantDna dna, Vector3 location)
    {
        var plant = new GameObject().AddComponent<Plant>();
        plant.PlantId = _lastPlantId++;
        plant.transform.position = location;
        plant.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);
        plant.PlantDna = dna;
        NewPlantEventBus.Publish(plant);

        if (LogReproductionSuccesses)
        {
            UnityEngine.Debug.Log($"Successfully planted {PlantApi.GetSpeciesPopulation(dna.SpeciesId)}th {dna.Name ?? "your plant"}.");
        }

        return plant;
    }

    public void ProcessDay()
    {
        var timer = new Stopwatch();
        timer.Start();
        while(timer.ElapsedMilliseconds < UpdateMilliseconds)
        {
            DropNextSeed();
        }
        timer.Stop();
    }

    public bool HasDayBeenProccessed()
    {
        return true;
    }


    private void DropNextSeed()
    {
        var seed = _seedQueue.Dequeue();
        if (IsLocationFertile(seed.Item2))
        {
            PlantSeed(seed.Item1, Singleton.LandService.ClampAboveTerrain(seed.Item2));
        }
    }

    private bool IsLocationFertile(Vector3 worldPosition)
    {
        var landService = Singleton.LandService;
        var landHeight = landService.SampleTerrainHeight(worldPosition);
        worldPosition.y = landHeight;
        var waterDepth = landService.SampleWaterDepth(worldPosition);
        var soilDepth = landService.SampleSoilDepth(worldPosition);
        var rootDepth = landService.SampleRootDepth(worldPosition);

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
            UnityEngine.Debug.Log($"Not enough {resource} to plant.");
        }
    }
}
