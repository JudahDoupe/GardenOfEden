using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using UnityEngine;

public class LightService : MonoBehaviour, IDailyProcess
{
    [Header("Settings")]
    [Range(1, 5)]
    public float UpdateMilliseconds = 5;
    [Range(100, 1000)]
    public int SimulationDensity = 100;
    public float CellWidth => ComputeShaderUtils.WorldSizeInMeters / SimulationDensity;

    public void AddAbsorber(Node node)
    {
        var coords = GetCoords(node);
        _absorberIndex.Add(node, coords);
        _lightAbsorberGrid[coords.Item1, coords.Item2].Append(node);
    }

    public void UpdateAbsorber(Node node)
    {
        RemoveAbsorber(node);
        AddAbsorber(node);
    }

    public void RemoveAbsorber(Node node)
    {
        if (_absorberIndex.TryGetValue(node, out var coords))
        {
            _absorberIndex.Remove(node);
            _lightAbsorberGrid[coords.Item1, coords.Item2].Remove(node);
        }
    }

    public void ProcessDay()
    {
        _hasDayBeenProcessed = false;
        StartCoroutine(ComputeAbsorpedLight());
    }

    public bool HasDayBeenProccessed()
    {
        return _hasDayBeenProcessed;
    }

    private Dictionary<Node, Tuple<int, int>> _absorberIndex;
    private List<Node>[,] _lightAbsorberGrid;

    private bool _hasDayBeenProcessed;
    private IEnumerator ComputeAbsorpedLight()
    {
        var timer = new Stopwatch();
        timer.Restart();

        for (int i = 0; i < SimulationDensity; i++)
        {
            for (int j = 0; j < SimulationDensity; j++)
            {
                if (timer.ElapsedMilliseconds > UpdateMilliseconds)
                {
                    yield return new WaitForEndOfFrame();
                    timer.Restart();
                }

                var remainingLightArea = CellWidth * CellWidth;
                foreach (var absorber in _lightAbsorberGrid[i, j].OrderByDescending(x => x.transform.position.y))
                {
                    var maxAbsobedLight = absorber.SurfaceArea * absorber.LightAbsorbtionRate;
                    var absorbedLight = Mathf.Min(remainingLightArea, maxAbsobedLight);
                    absorber.AbsorbedLight += absorbedLight;
                    remainingLightArea -= absorbedLight;

                    if (remainingLightArea <= Mathf.Epsilon)
                    {
                        break;
                    }
                }
            }
        }

        _hasDayBeenProcessed = true;
    }

    private void Start()
    {
        _lightAbsorberGrid = new List<Node>[SimulationDensity, SimulationDensity];
        for (int i = 0; i < SimulationDensity; i++)
        {
            for (int j = 0; j < SimulationDensity; j++)
            {
                _lightAbsorberGrid[i, j] = new List<Node>();
            }
        }
    }

    private Tuple<int,int> GetCoords(Node node)
    {
        return Tuple.Create(Mathf.FloorToInt((node.transform.position.x / CellWidth) % SimulationDensity),
                            Mathf.FloorToInt((node.transform.position.z / CellWidth) % SimulationDensity));
    } 
}
