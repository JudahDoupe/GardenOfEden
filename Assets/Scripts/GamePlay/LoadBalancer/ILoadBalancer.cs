using System;
using Assets.Scripts.Plants.Setup;

public interface ILoadBalancer
{
    UpdateChunk CurrentChunk { get; }
    UpdateChunk EnvironmentalChunk { get; }
    UpdateChunk ActiveEntityChunk { get; }
    UpdateChunk InactiveEntityChunk { get; }
    void RegisterEndSimulationAction(Action action);
}