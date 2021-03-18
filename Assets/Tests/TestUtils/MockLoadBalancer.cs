using System;
using Assets.Scripts.Plants.Setup;
using Unity.Mathematics;

namespace Tests
{
    public class MockLoadBalancer: ILoadBalancer
    {
        public UpdateChunk CurrentChunk { get; }
        public UpdateChunk EnvironmentalChunk { get; }
        public UpdateChunk ActiveEntityChunk { get; } 
        public UpdateChunk InactiveEntityChunk { get; }
        public float3 Position { get; } = float3.zero;
        public float Radius { get; } = 10000;

        public void RegisterEndSimulationAction(Action action)
        {
            throw new NotImplementedException();
        }
    }
}