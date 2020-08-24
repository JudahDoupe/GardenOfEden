using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    public void ProcessDay()
    {
        _hasDayBeenProcessed = false;
        StartCoroutine(ComputeAbsorpedLight());
    }

    public bool HasDayBeenProccessed()
    {
        return _hasDayBeenProcessed;
    }

    private Dictionary<Node, Tuple<int, int>> _absorberIndex = new Dictionary<Node, Tuple<int, int>>();
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
                    var maxAbsobedLight = absorber.SurfaceArea;
                    var absorbedLight = Mathf.Min(remainingLightArea, maxAbsobedLight);
                    remainingLightArea -= absorbedLight;
                    absorber.AbsorbedLight += absorbedLight * absorber.Dna.LightAbsorbtionRate;

                    if (remainingLightArea <= Mathf.Epsilon)
                    {
                        break;
                    }
                }
            }
        }

        _hasDayBeenProcessed = true;
    }

    private void Awake()
    {
        PlantMessageBus.NewNode.Subscribe(x => AddAbsorber(x));
        PlantMessageBus.NodeUpdate.Subscribe(x => UpdateAbsorber(x));
        PlantMessageBus.NodeDeath.Subscribe(x => RemoveAbsorber(x));

        _lightAbsorberGrid = new List<Node>[SimulationDensity, SimulationDensity];
        for (int i = 0; i < SimulationDensity; i++)
        {
            for (int j = 0; j < SimulationDensity; j++)
            {
                _lightAbsorberGrid[i, j] = new List<Node>();
            }
        }
    }

    private void AddAbsorber(Node node)
    {
        if (node.SurfaceArea <= Mathf.Epsilon) return;

        var coords = GetCoords(node);
        _absorberIndex.Add(node, coords);
        if (0 <= coords.Item1 && coords.Item1 < SimulationDensity
            && 0 <= coords.Item2 && coords.Item2 < SimulationDensity)
        {
            _lightAbsorberGrid[coords.Item1, coords.Item2].Add(node);
        }
    }

    private void UpdateAbsorber(Node node)
    {
        RemoveAbsorber(node);
        AddAbsorber(node);
    }

    private void RemoveAbsorber(Node node)
    {
        if (_absorberIndex.TryGetValue(node, out var coords))
        {
            _absorberIndex.Remove(node);
            _lightAbsorberGrid[coords.Item1, coords.Item2].Remove(node);
        }
    }

    private Tuple<int,int> GetCoords(Node node)
    {
        var uv = ComputeShaderUtils.LocationToUv(node.transform.position);
        var id = uv * SimulationDensity;
        return Tuple.Create(Mathf.FloorToInt(id.x), Mathf.FloorToInt(id.y));
    } 
}
