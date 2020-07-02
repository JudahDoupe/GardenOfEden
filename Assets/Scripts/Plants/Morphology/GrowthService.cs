using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrowthService : MonoBehaviour
{
    public bool SmoothGrow = true;
    public LinkedList<Plant> UpdateQueue = new LinkedList<Plant>();

    private MophologyGrowthVisitor _growthVisitor = new MophologyGrowthVisitor();
    private VisualGrowthVisitor _meshVisitor = new VisualGrowthVisitor(0);

    private void Start()
    {
        if (SmoothGrow)
        {
            _meshVisitor = new VisualGrowthVisitor(EnvironmentApi.Instance.SecondsPerDay);
        }
    }
    private void Update()
    {
        var plant = UpdateQueue.FirstOrDefault(x => !x.IsGrowing);
        if (plant != null)
        {
            UpdateQueue.Remove(plant);
            if (Mathf.FloorToInt(EnvironmentApi.GetDate()) > Mathf.FloorToInt(plant.lastUpdateDate))
            {
                plant.Accept(_growthVisitor);
                plant.Accept(_meshVisitor);
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
