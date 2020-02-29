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

    public List<Plant> GetLivingPlants()
    {
        return _livingPlants.ToList();
    }

    /* Inner Mechinations */

    private LinkedList<Plant> _livingPlants = new LinkedList<Plant>();
    private int _currentFrame = 0;

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

            if (DI.GameService.FocusedPlant == plant && !plant.IsMature)
                _livingPlants.AddFirst(plant);
            else
                _livingPlants.AddLast(plant);

            if (SustainLife(plant))
            {
                plant.GrowthState.Grow(plant);
                plant.LastUpdatedDate = EnvironmentApi.GetDate();
            }
            else
            {
                plant.Die();
            }
        }
    }

    private bool SustainLife(Plant plant)
    {
        var delatTime = EnvironmentApi.GetDate() - plant.LastUpdatedDate;
        var usedSugar = plant.SustainingSugar._cubicMeters * delatTime;
        plant.StoredStarch -= Volume.FromCubicMeters(usedSugar);
        return plant.StoredStarch > Volume.FromCubicMeters(0);
    }
}
