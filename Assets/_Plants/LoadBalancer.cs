using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Setup;
using Unity.Mathematics;
using UnityEngine;

public class LoadBalancer : Singleton<LoadBalancer>
{
    [Range(10,60)]
    public int TargetFps = 20;

    public static UpdateChunk CurrentChunk { get; private set; }
    public static UpdateChunk EnvironmentalChunk { get; private set; }
    public static UpdateChunk ActiveEntityChunk { get; private set; }
    public static UpdateChunk InactiveEntityChunk { get; private set; }
    public static float3 Position => CameraController.Instance.Focus.position;
    public static float Radius { get; private set; }

    private static List<Action> _environmentalSystems = new List<Action>();
    private static float[] _deltaTimes = new float[7];

    public void Start()
    {
        EnvironmentalChunk = new UpdateChunk { Id = -1 };
        ActiveEntityChunk = new UpdateChunk { Id = 1 };
        InactiveEntityChunk = new UpdateChunk { Id = 2 };

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
            _deltaTimes[TimeService.DayOfTheWeek] = Time.deltaTime;
            var averageDeltaTime = _deltaTimes.Average();
            var targetDeltaTime = 1f / TargetFps;
            var surfaceArea = math.PI * math.pow(Radius, 2);
            var targetSurfaceArea = surfaceArea * (targetDeltaTime / averageDeltaTime);
            var targetRadius = Radius * math.sqrt(targetSurfaceArea / math.PI) / Radius;
            Radius = math.clamp(targetRadius, 10, Coordinate.PlanetRadius);
        }

        CurrentChunk = CurrentChunk.IsEnvironmental ? ActiveEntityChunk : EnvironmentalChunk;
    }

    public static void RegisterEndSimulationAction(Action action)
    {
        _environmentalSystems.Add(action);
    }
}