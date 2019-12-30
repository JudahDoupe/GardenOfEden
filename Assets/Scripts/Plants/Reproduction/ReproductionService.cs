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

        plant.Dna = dna;

        plant.GrowthState = new PrimaryGrowthState();
        plant.StoredSugar = Volume.FromCubicMeters(3);

        if (LogReproductionSuccesses)
        {
            Debug.Log($"Successfully planted {PlantApi.GetSpeciesPopulation(dna.SpeciesId)}th {dna.Name ?? "your plant"}.");
        }

        return plant;
    }

    /* Inner Mechinations */

    private CameraController _camera;

    private int lastPlantId = 10;
    private Queue<Tuple<PlantDna, Vector3>> _seedQueue = new Queue<Tuple<PlantDna, Vector3>>();

    void Start()
    {
        _camera = FindObjectOfType<CameraController>();
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
        var fertileLocation = GetFirtileLocation(seed.Item2);
        if (fertileLocation.HasValue)
        {
            PlantSeed(seed.Item1, fertileLocation.Value);
        }
    }

    private Vector3? GetFirtileLocation(Vector3 worldPosition)
    {
        var ray = new Ray(worldPosition + Vector3.up * 10, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Soil")))
        {
            var waterDepth = EnvironmentApi.SampleWaterDepth(hit.point);
            var soilDepth = EnvironmentApi.SampleSoilDepth(hit.point);
            var rootDepth = PlantApi.SampleRootDepth(hit.point);

            if (soilDepth < 0.05f)
                DebugLackOfReources("soil");
            else if (rootDepth > 0.01f)
                DebugLackOfReources("root space");
            else if (waterDepth < 0.1f)
                DebugLackOfReources("water");
            else
                return hit.point;
        }
        else
        {
            DebugLackOfReources("fertile soil");
        }

        return null;
    }

    private void DebugLackOfReources(string resource)
    {
        if (LogReproductionFailures)
        {
            Debug.Log($"Not enough {resource} to plant.");
        }
    }

}
