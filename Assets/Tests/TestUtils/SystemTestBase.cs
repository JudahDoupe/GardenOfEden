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
            _config.MaxNbOfTest = 100;
        }

        public static Gen<float> Gen0To1()
        {
            return Gen.Choose(0, 1000000).Select(x => x / 1000000f);
        }
        public static Gen<float> GenNeg1To1()
        {
            return Gen.Choose(-1000000, 1000000).Select(x => x / 1000000f);
        }
    }
}