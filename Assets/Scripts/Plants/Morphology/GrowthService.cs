using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class GrowthService : MonoBehaviour, IDailyProcess
{
    public float UpdateMilliseconds = 3;
    public float SmoothGrowDistance = 20;

    public int GrowingPlantCount => _growingPlants.Count();

    private List<Plant> _growingPlants = new List<Plant>();
    private MophologyGrowthVisitor _growthVisitor = new MophologyGrowthVisitor();
    private VisualGrowthVisitor _smoothMeshVisitor = new VisualGrowthVisitor(2);
    private VisualGrowthVisitor _fastMeshVisitor = new VisualGrowthVisitor(0);

    private bool _hasDayBeenProcessed = false;

    private void Awake()
    {
        PlantMessageBus.NewPlant.Subscribe(x => _growingPlants.Add(x));
        PlantMessageBus.PlantDeath.Subscribe(x => _growingPlants.Remove(x));
    }

    public void ProcessDay()
    {
        _hasDayBeenProcessed = false;
        _growingPlants.OrderBy(x => Vector3.Distance(Camera.main.transform.position, x.transform.position));
        StartCoroutine(GrowPlants());
    }

    public bool HasDayBeenProccessed()
    {
        return _hasDayBeenProcessed && !_growingPlants.Any(x => x.IsGrowing);
    }

    private IEnumerator GrowPlants()
    {
        var updateQueue = new Queue<Plant>(_growingPlants);
        while (updateQueue.Any())
        {
            var updateTimer = new Stopwatch();
            updateTimer.Restart();
            while (updateQueue.Any() && updateTimer.ElapsedMilliseconds < UpdateMilliseconds)
            {
                var plant = updateQueue.Dequeue();
                if (plant != null)
                {
                    var meshVisitor = Vector3.Distance(Camera.main.transform.position, plant.transform.position) > SmoothGrowDistance ? _fastMeshVisitor : _smoothMeshVisitor;
                    plant.Accept(_growthVisitor);
                    plant.Accept(meshVisitor);
                    yield return new WaitForEndOfFrame();
                }
            }
            updateTimer.Stop();
        }
        _hasDayBeenProcessed = true;
    }
}
