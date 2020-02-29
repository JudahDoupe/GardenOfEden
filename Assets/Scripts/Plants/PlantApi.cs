using System;
using System.Linq;
using UnityEngine;

public class PlantApi : MonoBehaviour
{
    /* API */

    public static float SampleRootDepth(Vector3 location)
    {
        return _rootService.SampleRootDepth(location);
    }
    public static void DropSeed(PlantDna dna, Vector3 location)
    {
        _reproductionService.DropSeed(dna, location);
    }
    public static void StartPlantGrowth(Plant plant)
    {
        _growthService.StartPlantGrowth(plant);
    }

    public static int GetSpeciesPopulation(int speciesId)
    {
        return FindObjectsOfType<Plant>().Count(p => p.Dna.SpeciesId == speciesId);
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
