using FsCheck;
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
            Singleton.LoadBalancer = new MockLoadBalancer();
        }
    }
}