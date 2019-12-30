using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;

public class GrowthService : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 60)]
    public int FramesPerPlantGrowth = 1;

    /* Publicly Accessible Methods */

    public Subject<Plant> NewPlantSubject = new Subject<Plant>();
    public Subject<Plant> PlantDeathSubject = new Subject<Plant>();

    public void StartPlantGrowth(Plant plant)
    {
        NewPlantSubject.Publish(plant);
        _livingPlants.AddLast(plant);
    }

    public void StopPlantGrowth(Plant plant)
    {
        PlantDeathSubject.Publish(plant);
        _livingPlants.Remove(plant);
    }

    /* Inner Mechinations */

    private LinkedList<Plant> _livingPlants = new LinkedList<Plant>();
    private int _currentFrame = 0;

    private GameService _gameService;

    void Start()
    {
        _gameService = FindObjectOfType<GameService>();
    }
    void Update()
    {
        if(_currentFrame++ % FramesPerPlantGrowth == 0)
            GrowNextPlant();
    }

    private void GrowNextPlant()
    {
        if (_livingPlants.Any(x => !x.IsGrowing))
        {
            var plant = _livingPlants.First(x => !x.IsGrowing);
            _livingPlants.Remove(plant);

            if (_gameService.FocusedPlant == plant && !plant.IsMature)
                _livingPlants.AddFirst(plant);
            else
                _livingPlants.AddLast(plant);

            PlantApi.UpdateWater(plant);
            GenerateSugar(plant);

            if (SustainLife(plant))
            {
                PlantApi.UpdateRoots(plant);
                plant.GrowthState.Grow(plant);
                plant.LastUpdatedDate = EnvironmentApi.GetDate();
            }
            else
            {
                PlantApi.KillPlant(plant);
            }
        }
    }

    private bool SustainLife(Plant plant)
    {
        var delatTime = EnvironmentApi.GetDate() - plant.LastUpdatedDate;
        var usedSugar = plant.SustainingSugar._cubicMeters * delatTime;
        plant.StoredSugar -= Volume.FromCubicMeters(usedSugar);
        return plant.StoredSugar > Volume.FromCubicMeters(0);
    }

    private void GenerateSugar(Plant plant)
    {
        var waterPerSugar = 3.0f; //TODO: store this value in the leaves

        var availableLight = EnvironmentApi.GetAbsorpedLight(plant.PlantId);
        var availableWater = plant.StoredWater;

        var requestedLight = availableWater / waterPerSugar;

        var usedLight = requestedLight;
        if (availableLight < requestedLight)
            usedLight = availableLight;
        var usedWater = usedLight * waterPerSugar;

        plant.StoredWater -= usedWater;
        plant.StoredSugar += usedWater / waterPerSugar * 1;
    }
}
