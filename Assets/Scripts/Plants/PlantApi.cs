using System;
using System.Linq;
using UnityEngine;

public class PlantApi : MonoBehaviour
{
    /* API */

    public static void UpdateWater(Plant plant)
    {
        var newWater = _rootService.AbsorpWater(plant);
        plant.StoredWater += newWater;
    }
    public static void UpdateRoots(Plant plant)
    {
        _rootService.SpreadRoots(plant, plant.RootRadius, plant.AgeInDay);
    }
    public static void KillPlant(Plant plant)
    {
        _rootService.RemoveRoots(plant);
        _growthService.StopPlantGrowth(plant);
        plant.IsAlive = false;
        Destroy(plant.gameObject);
    }

    public static float SampleRootDepth(Vector3 location)
    {
        return _rootService.SampleRootDepth(location);
    }
    public static void DropSeed(PlantDNA dna, Vector3 location)
    {
        _reproductionService.DropSeed(dna, location);
    }
    public static void StartPlantGrowth(Plant plant)
    {
        _growthService.StartPlantGrowth(plant);
    }

    public static int GetSpeciesPopulation(Guid speciesId)
    {
        return FindObjectsOfType<Plant>().Count(p => p.DNA.SpeciesId == speciesId);
    }
    public static int GetTotalPlantPopulation()
    {
        return FindObjectsOfType<Plant>().Count();
    }

    /* INNER MECHINATIONS */

    public static PlantApi Instance;
    private static RootService _rootService;
    private static GrowthService _growthService;
    private static ReproductionService _reproductionService;

    void Awake()
    {
        Instance = this;
        _rootService = GetComponent<RootService>();
        _growthService = GetComponent<GrowthService>();
        _reproductionService = GetComponent<ReproductionService>();
    }
}
