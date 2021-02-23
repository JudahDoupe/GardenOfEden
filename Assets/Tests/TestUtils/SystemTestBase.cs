using System;
using System.Collections.Generic;
using FsCheck;
using NUnit.Framework;
using Unity.Entities.Tests;

namespace Tests
{
    public class SystemTestBase: ECSTestsFixture
    {
        public Configuration _config;

        [SetUp]
        public void SetUp()
        {
            base.Setup();
            Singleton.Land = new MockLandService();
            Singleton.LoadBalancer = new MockLoadBalancer();

            _config = Configuration.QuickThrowOnFailure;
            _config.MaxNbOfTest = 10;
        }
    }
}