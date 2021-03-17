using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Setup;
using Unity.Mathematics;
using UnityEngine;

public class ProximityLoadBalancer : MonoBehaviour, ILoadBalancer
{
    [Range(0.005f, 0.06f)]
    public float TargetDeltaTime = 0.3f;

    public UpdateChunk CurrentChunk { get; private set; }
    public UpdateChunk EnvironmentalChunk { get; private set; }
    public UpdateChunk ActiveEntityChunk { get; private set; }
    public UpdateChunk InactiveEntityChunk { get; private set; }

    private List<Action> _environmentalSystems = new List<Action>();
    private float[] _deltaTimes = new float[7];

    public void Start()
    {
        EnvironmentalChunk = new UpdateChunk
        {
            Id = -1,
            Position = new float3(0,0,0)
        };
        ActiveEntityChunk = new UpdateChunk
        {
            Id = 1,
            Position = Camera.main.transform.position,
            Radius = 1000
        };
        InactiveEntityChunk = new UpdateChunk
        {
            Id = 2,
            Position = new float3(0, 0, 0)
        };

        CurrentChunk = EnvironmentalChunk;
    }

    public void Update()
    {
        if (CurrentChunk.IsEnvironmental)
        {
            foreach (var runEnvironmentalSystem in _environmentalSystems)
            {
                runEnvironmentalSystem();
            }
        }
        else
        {
            _deltaTimes[Singleton.TimeService.DayOfTheWeek] = Time.deltaTime;
            var newChunk = ActiveEntityChunk;
            newChunk.Position = Singleton.CameraController.FocusPos;
            ActiveEntityChunk = newChunk;
        }

        if (Singleton.TimeService.DayOfTheWeek == 6)
        {
            BalanceChunks();
        }

        CurrentChunk = CurrentChunk.IsEnvironmental ? ActiveEntityChunk : EnvironmentalChunk;
    }

    public void RegisterEndSimulationAction(Action action)
    {
        _environmentalSystems.Add(action);
    }

    public void BalanceChunks()
    {
        var averageDeltaTime = _deltaTimes.Average();
        var sa = math.PI * math.pow(ActiveEntityChunk.Radius, 2);
        var targetSa = sa * (TargetDeltaTime / averageDeltaTime);
        var newChunk = ActiveEntityChunk;
        newChunk.Radius *= math.sqrt(targetSa / math.PI) / ActiveEntityChunk.Radius;
        newChunk.Radius = math.clamp(newChunk.Radius, 10, Coordinate.PlanetRadius);
        ActiveEntityChunk = newChunk;
    }
}