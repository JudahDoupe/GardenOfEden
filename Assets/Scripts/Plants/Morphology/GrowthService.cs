using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class GrowthService : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 5)]
    public float UpdateMilliseconds = 5;
    public bool SmoothGrow = true;

    private LinkedList<Plant> _updateQueue = new LinkedList<Plant>();
    private MophologyGrowthVisitor _growthVisitor = new MophologyGrowthVisitor();
    private VisualGrowthVisitor _meshVisitor = new VisualGrowthVisitor(0);

    public void AddPlant(Plant plant)
    {
        _updateQueue.AddFirst(plant);
    }

    public void RemovePlant(Plant plant)
    {
        _updateQueue.Remove(plant);
    }


    private void Start()
    {
        if (SmoothGrow)
        {
            _meshVisitor = new VisualGrowthVisitor(EnvironmentApi.Instance.SecondsPerDay);
        }
    }
    private void Update()
    {
        var updateTimer = new Stopwatch();
        updateTimer.Restart();
        while (updateTimer.ElapsedMilliseconds < UpdateMilliseconds)
        {
            GrowNextPlant();
        }
        updateTimer.Stop();
    }

    private void GrowNextPlant()
    {
        var plant = _updateQueue.FirstOrDefault(x => !x.IsGrowing
                        && Mathf.FloorToInt(EnvironmentApi.GetDate()) > Mathf.FloorToInt(x.lastUpdateDate));
        if (plant != null)
        {
            _updateQueue.Remove(plant);

            var missedDays = Mathf.FloorToInt(EnvironmentApi.GetDate()) - Mathf.FloorToInt(plant.lastUpdateDate);
            var timer = new Stopwatch();
            timer.Restart();
            plant.Accept(_growthVisitor);
            plant.Accept(_meshVisitor);
            plant.lastUpdateDate = EnvironmentApi.GetDate();

            _updateQueue.AddLast(plant);
        }
    }
}
