/*
using System.Collections.Generic;
using Assets.Scripts.Plants.Growth;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class EnergyFlowSystemTests : SystemTestBase
    {
        public static Gen<EnergyStore> GenEnergyStore(float maxCapacity = 25) =>
            from capacity in FsCheckUtils.GenFloat(0, maxCapacity)
            from quantity in FsCheckUtils.GenFloat(0, capacity)
            select new EnergyStore { Capacity = capacity, Quantity = quantity };

        public static Gen<EnergyFlow> GenEnergyFlow(float minCapacity, float maxCapacity) =>
            from throughput in FsCheckUtils.GenFloat(minCapacity, maxCapacity)
            select new EnergyFlow { Throughput = throughput };

        private static Gen<TestData> GenTestData() =>
            from node in GrowthSystemTests.GenNode()
            from energyStore in GenEnergyStore(node.Volume)
            from energyFlow in GenEnergyFlow(0, energyStore.Capacity)
            select new TestData { EnergyFlow = energyFlow, EnergyStore = energyStore, Node = node };

        private static Gen<TestData[]> GenTestDataArray() =>
            from n in Gen.Choose(2, 10)
            from array in Gen.ArrayOf(n, GenTestData())
            select array;

        [Test]
        public void EnergyShouldFlowInBothDirections()
        {
            Prop.ForAll(Gen.ArrayOf(2, GenTestData()).ToArbitrary(), data =>
            {
                foreach(var d in data)
                {
                    d.EnergyFlow.Throughput = 0;
                }

                var bottomData = data[0];
                var bottomEntity = CreateNode(bottomData, Entity.Null);

                var topData = data[1];
                var topEntity = CreateNode(topData, bottomEntity);

                World.GetOrCreateSystem<EnergyFlowSystem>().Update();

                m_Manager.GetComponentData<EnergyFlow>(bottomEntity).Throughput.Should().Be(0);
                var throughput = m_Manager.GetComponentData<EnergyFlow>(topEntity).Throughput;
                var bottomQuantity = m_Manager.GetComponentData<EnergyStore>(bottomEntity).Quantity;
                var topQuantity = m_Manager.GetComponentData<EnergyStore>(topEntity).Quantity;

                if (bottomData.EnergyStore.Pressure > topData.EnergyStore.Pressure)
                {
                    throughput.Should().BeGreaterThan(0);
                }
                else
                {
                    throughput.Should().BeLessOrEqualTo(0);
                }

                if (bottomData.Node.Volume > bottomData.EnergyStore.Quantity - throughput)
                {
                    bottomQuantity.Should().BeApproximately(bottomData.EnergyStore.Quantity - throughput, 0.000001f, "throughput should be subtracted from bottom node");
                }
                else
                {
                    bottomQuantity.Should().BeApproximately(bottomData.Node.Volume, 0.000001f, "bottom quantity should not exceed capacity");
                }

                if (topData.Node.Volume > topData.EnergyStore.Quantity + throughput)
                {
                    topQuantity.Should().BeApproximately(topData.EnergyStore.Quantity + throughput, 0.000001f, "throughput should be added to top node");
                }
                else
                {
                    topQuantity.Should().BeApproximately(topData.Node.Volume, 0.000001f, "top quantity should not exceed capacity");
                }

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void EnergyThroughputShouldBeRelativeToNumberOfConnections () 
        {
            Prop.ForAll(GenTestDataArray().ToArbitrary(), data =>
            {
                var bottom = Entity.Null;
                var nodes = new List<Entity>();
                var maxThroughput = 0f;

                foreach (var d in data)
                {
                    d.EnergyFlow.Throughput = 0;
                    var node = CreateNode(d, bottom);
                    if (bottom == Entity.Null)
                    {
                        bottom = node;
                        maxThroughput = d.EnergyStore.Capacity / (data.Length - 1);
                    }
                    else
                    {
                        nodes.Add(node);
                    }
                }

                World.GetOrCreateSystem<EnergyFlowSystem>().Update();

                foreach( var node in nodes)
                {
                    var throughput = m_Manager.GetComponentData<EnergyFlow>(node).Throughput;
                    throughput.Should().BeLessOrEqualTo(maxThroughput);
                }

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void NodeQuantityShouldNotExceedNodeCapacity()
        {
            Prop.ForAll(Gen.ArrayOf(2, GenTestData()).ToArbitrary(), data =>
            {
                var bottom = CreateNode(data[0], Entity.Null);
                var top = CreateNode(data[1], bottom);

                World.GetOrCreateSystem<EnergyFlowSystem>().Update();

                var bottomStore = m_Manager.GetComponentData<EnergyStore>(bottom);
                bottomStore.Quantity.Should().BeLessOrEqualTo(bottomStore.Capacity);
                var topStore = m_Manager.GetComponentData<EnergyStore>(top);
                topStore.Quantity.Should().BeLessOrEqualTo(topStore.Capacity);

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void CapacityGetsRecalculatedBasedOnNodeVolume()
        {
            Prop.ForAll(Gen.ArrayOf(2, GenTestData()).ToArbitrary(), data =>
            {
                var bottom = CreateNode(data[0], Entity.Null);
                var top = CreateNode(data[1], bottom);

                World.GetOrCreateSystem<EnergyFlowSystem>().Update();

                var bottomNode = m_Manager.GetComponentData<Node>(bottom);
                var bottomStore = m_Manager.GetComponentData<EnergyStore>(bottom);
                bottomStore.Capacity.Should().BeApproximately(math.max(bottomNode.Volume, 0.001f), 0.000001f);
                var topNode = m_Manager.GetComponentData<Node>(top);
                var topStore = m_Manager.GetComponentData<EnergyStore>(top);
                topStore.Capacity.Should().BeApproximately(math.max(topNode.Volume, 0.001f), 0.000001f);

            }).Check(FsCheckUtils.Config);
        }

        private Entity CreateNode(TestData data, Entity parent)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Parent { Value = parent });
            m_Manager.AddComponentData(entity, data.EnergyStore);
            m_Manager.AddComponentData(entity, data.EnergyFlow);
            m_Manager.AddComponentData(entity, data.Node);
            m_Manager.AddBuffer<Child>(entity);
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);

            if (parent != Entity.Null)
            {
                var children = m_Manager.GetBuffer<Child>(parent);
                children.Add(new Child { Value = entity });
            }
            return entity;
        }

        private class TestData
        {
            public EnergyStore EnergyStore;
            public EnergyFlow EnergyFlow;
            public Node Node;
        }
    }
}

*/