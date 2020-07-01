using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantGrowthService : MonoBehaviour
{
    public LinkedList<Plant> UpdateQueue = new LinkedList<Plant>();

    private GrowthFairy _fairy = new GrowthFairy();

    private void Update()
    {
        var plant = UpdateQueue.FirstOrDefault(x => !x.IsGrowing);
        if (plant != null)
        {
            UpdateQueue.Remove(plant);
            if (Mathf.FloorToInt(EnvironmentApi.GetDate()) > Mathf.FloorToInt(plant.lastUpdateDate))
            {
                plant.Accept(_fairy);
                plant.UpdateMesh(EnvironmentApi.Instance.SecondsPerDay);
                plant.lastUpdateDate = EnvironmentApi.GetDate();
            }
            UpdateQueue.AddLast(plant);
        }
    }

    public void AddPlant(Plant plant)
    {
        UpdateQueue.AddFirst(plant);
    }

    public void RemovePlant(Plant plant)
    {
        UpdateQueue.Remove(plant);
    }
}
