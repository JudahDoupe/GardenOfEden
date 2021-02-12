using System;
using System.Collections.Generic;
using Assets.Scripts.Plants.Setup;

namespace Tests
{
    public class MockLoadBalancer: ILoadBalancer
    {
        public UpdateChunk CurrentChunk { get; }
        public List<UpdateChunk> UpdateChunks { get; }
        public void RegisterEndSimulationAction(Action action)
        {
            throw new NotImplementedException();
        }

        public void BalanceChunks()
        {
            throw new NotImplementedException();
        }
    }
}