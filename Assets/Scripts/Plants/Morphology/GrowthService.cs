using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GrowthService : MonoBehaviour, IDailyProcess
{
    public float UpdateMilliseconds = 3;
    public float SmoothGrowDistance = 20;

    public int GrowingPlantCount => _growingPlants.Count();

    private List<Plant> _growingPlants = new List<Plant>();
    private MophologyGrowthVisitor _growthVisitor = new MophologyGrowthVisitor();
    private VisualGrowthVisitor _smoothMeshVisitor = new VisualGrowthVisitor(0);
    private VisualGrowthVisitor _fastMeshVisitor = new VisualGrowthVisitor(0);

    private bool _hasDayBeenProcessed = false;
    private float _growTime = 0.25f;

    private void Awake()
    {
        PlantMessageBus.NewPlant.Subscribe(x => _growingPlants.Add(x));
        PlantMessageBus.PlantDeath.Subscribe(x => _growingPlants.Remove(x));
    }

    public void ProcessDay()
    {
        _hasDayBeenProcessed = false;
        _growingPlants.OrderBy(x => Vector3.Distance(Camera.main.transform.position, x.transform.position));
        _smoothMeshVisitor = new VisualGrowthVisitor(Mathf.Max(_growTime, 0.3f));
        StartCoroutine(GrowPlants());
    }

    public bool HasDayBeenProccessed()
    {
        return _hasDayBeenProcessed && _growingPlants.All(x => !x.IsGrowing);
    }

    private IEnumerator GrowPlants()
    {
        var growTimer = new Stopwatch();
        growTimer.Restart();
        
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
        _growTime = (float)growTimer.Elapsed.TotalSeconds * 0.9f;
    }
}
