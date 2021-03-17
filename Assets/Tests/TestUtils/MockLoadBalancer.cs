using System;
using System.Collections.Generic;
using Assets.Scripts.Plants.Setup;

namespace Tests
{
    public class MockLoadBalancer: ILoadBalancer
    {
        public UpdateChunk CurrentChunk { get; }
        public UpdateChunk EnvironmentalChunk { get; }
        public UpdateChunk ActiveEntityChunk { get; }
        public UpdateChunk InactiveEntityChunk { get; }
        public void RegisterEndSimulationAction(Action action)
        {
            throw new NotImplementedException();
        }
    }
}