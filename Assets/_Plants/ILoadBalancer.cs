using System;
using Assets.Scripts.Plants.Setup;
using Unity.Mathematics;

public interface ILoadBalancer
{
    UpdateChunk CurrentChunk { get; }
    UpdateChunk EnvironmentalChunk { get; }
    UpdateChunk ActiveEntityChunk { get; }
    UpdateChunk InactiveEntityChunk { get; }
    float3 Position { get; }
    float Radius { get; }
    void RegisterEndSimulationAction(Action action);
}