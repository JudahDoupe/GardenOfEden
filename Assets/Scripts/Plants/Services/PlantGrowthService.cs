using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantGrowthService : MonoBehaviour
{
    public LinkedList<Plant> UpdateQueue = new LinkedList<Plant>();

    private void Update()
    {
        var plant = UpdateQueue.First();
        UpdateQueue.RemoveFirst();
        if (Mathf.FloorToInt(EnvironmentApi.GetDate()) > Mathf.FloorToInt(plant.lastUpdateDate))
        {
            plant.Accept(new GrowthFairy());
            plant.lastUpdateDate = EnvironmentApi.GetDate();
        }
        UpdateQueue.AddLast(plant);
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
