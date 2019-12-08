using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrowthService : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 60)]
    public int FramesPerPlantGrowth = 1;

    /* Pubically Accessable Methods */

    public void StartPlantGrowth(Plant plant)
    {
        _livingPlants.Add(plant);
    }

    public void StopPlantGrowth(Plant plant)
    {
        _livingPlants.Remove(plant);
    }

    /* Inner Mechinations */

    private List<Plant> _livingPlants = new List<Plant>();

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
            _livingPlants.Add(plant);

            PlantApi.UpdateWater(plant);
            //TODO: update energy

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
        //TODO: Use energy to sustain life based on number of structures
        return true;
    }
}
