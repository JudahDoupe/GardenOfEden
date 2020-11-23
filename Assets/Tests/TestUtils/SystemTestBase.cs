using System;
using System.Collections.Generic;
using Assets.Scripts.Plants.Systems;
using NUnit.Framework;
using Unity.Entities.Tests;

namespace Tests
{
    public class SystemTestBase: ECSTestsFixture
    {
        [SetUp]
        public void SetUp()
        {
            base.Setup();
            Singleton.LandService = new MockLandService();
            Singleton.LoadBalancer = new MockLoadBalancer();
        }
    }
}