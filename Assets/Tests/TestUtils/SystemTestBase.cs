using System;
using System.Collections.Generic;
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
            Singleton.Land = new MockLandService();
            Singleton.LoadBalancer = new MockLoadBalancer();
        }
    }
}