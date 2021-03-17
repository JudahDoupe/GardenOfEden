using Assets.Scripts.Plants.Growth;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Tests;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class GrowthSystemTests : SystemTestBase
    {
        public static Gen<Node> GenNode() =>
            from size in FsCheckUtils.GenFloat3(new float3(0,0,0), new float3(1,1,1))
            from radius in FsCheckUtils.GenFloat(0, 0.5f)
            from length in FsCheckUtils.GenFloat(0, 2)
            select new Node { Size = size, InternodeLength = length, InternodeRadius = radius };

        public static Gen<PrimaryGrowth> GenPrimaryGrowth() =>
            from size in FsCheckUtils.GenFloat3(new float3(0, 0, 0), new float3(1, 1, 1))
            from radius in FsCheckUtils.GenFloat(0, 0.5f)
            from length in FsCheckUtils.GenFloat(0, 2)
            from dtm in Gen.Choose(1,50)
            select new PrimaryGrowth { NodeSize = size, InternodeLength = length, InternodeRadius = radius, DaysToMature = dtm };

        private static Gen<TestData> GenTestData() =>
            from node in GenNode()
            from growth in GenPrimaryGrowth()
            from energyStore in EnergyFlowSystemTests.GenEnergyStore()
            select new TestData { 
                Node = node,
                PrimaryGrowth = growth, 
                Translation = new Translation { Value = new float3(0, 0, node.InternodeLength) },
                EnergyStore = energyStore 
            };

        [Test]
        public void PrimaryGrowthGrowsInDaysToMature()
        {
            Prop.ForAll(GenTestData().ToArbitrary(), data =>
            {
                data.EnergyStore = new EnergyStore { Quantity = 1000 };
                data.Node = new Node { Size = new float3(0,0,0), InternodeLength = 0, InternodeRadius = 0 };
                var originalVolume = data.Node.Volume;

                var entity = CreateNode(data);
                var elapsedDays = 0;
                while (m_Manager.GetComponentData<Node>(entity).Volume < data.PrimaryGrowth.Volume)
                {
                    elapsedDays++;
                    World.GetOrCreateSystem<GrowthSystem>().Update();
                    var expectedVolume = (data.PrimaryGrowth.Volume / data.PrimaryGrowth.DaysToMature) * elapsedDays;
                    var currentVolume = m_Manager.GetComponentData<Node>(entity).Volume;
                    currentVolume.Should().BeApproximately(expectedVolume, 0.00001f, "volume should have grown");
                }

                elapsedDays.Should().Be(data.PrimaryGrowth.DaysToMature);

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void PrimaryGrowthStopsWhenGreaterThanMaxSize()
        {
            Prop.ForAll(GenTestData().ToArbitrary(), data =>
            {
                data.EnergyStore = new EnergyStore { Quantity = 1000 };
                var originalVolume = data.Node.Volume;

                var entity = CreateNode(data);

                World.GetOrCreateSystem<GrowthSystem>().Update();

                var currentVolume = m_Manager.GetComponentData<Node>(entity).Volume;
                var expectedGrowth = data.PrimaryGrowth.Volume / data.PrimaryGrowth.DaysToMature;
                if (originalVolume > data.PrimaryGrowth.Volume)
                {
                    currentVolume.Should().BeApproximately(originalVolume, 0.000001f, "growth system should not have run");
                }
                else if (originalVolume + expectedGrowth > data.PrimaryGrowth.Volume)
                {
                    currentVolume.Should().BeApproximately(data.PrimaryGrowth.Volume, 0.000001f, "volume should not exceed primary growth volume");
                }
                else
                {
                    currentVolume.Should().BeGreaterThan(originalVolume, "volume should have grown");
                    currentVolume.Should().BeLessOrEqualTo(data.PrimaryGrowth.Volume, "volume should not exceed primary growth volume");
                }

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void OnlyUsesHalfItsEnergyStoreToGrow()
        {
            Prop.ForAll(GenTestData().ToArbitrary(), data =>
            {
                var originalEnergy = data.EnergyStore;

                var entity = CreateNode(data);

                World.GetOrCreateSystem<GrowthSystem>().Update();

                var energy = m_Manager.GetComponentData<EnergyStore>(entity);
                energy.Quantity.Should().BeGreaterOrEqualTo(data.EnergyStore.Quantity / 2);

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void GrowingInternodeMovesNode()
        {
            Prop.ForAll(GenTestData().ToArbitrary(), data =>
            {
                var originalTranslation = data.Translation;
                var originalNode = data.Node;

                var entity = CreateNode(data);

                World.GetOrCreateSystem<GrowthSystem>().Update();

                var node = m_Manager.GetComponentData<Node>(entity);
                var translation = m_Manager.GetComponentData<Translation>(entity);
                translation.Value.z.Should().BeApproximately(node.InternodeLength, 0.000001f);

            }).Check(FsCheckUtils.Config);
        }

        private Entity CreateNode(TestData data)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, data.PrimaryGrowth);
            m_Manager.AddComponentData(entity, data.Translation);
            m_Manager.AddComponentData(entity, data.EnergyStore);
            m_Manager.AddComponentData(entity, data.Node);
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
            return entity;
        }

        private class TestData
        {
            public Node Node;
            public PrimaryGrowth PrimaryGrowth;
            public Translation Translation;
            public EnergyStore EnergyStore;
        }
    }
}
