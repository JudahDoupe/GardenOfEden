using System.Collections.Generic;
using Assets.Scripts.Plants.Growth;
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
        public static Gen<EnergyStore> GenEnergyStore() =>
            from capacity in FsCheckUtils.GenFloat(0, 25)
            from quantity in FsCheckUtils.GenFloat(0, capacity)
            select new EnergyStore { Capacity = capacity, Quantity = quantity };

        public static Gen<EnergyFlow> GenEnergyFlow(float minCapacity, float maxCapacity) =>
            from throughput in FsCheckUtils.GenFloat(minCapacity, maxCapacity)
            select new EnergyFlow { Throughput = throughput };

        const float InternodeCapacity = 3.141529f;
        const float NodeCapacity = 4.187743f;
        private const float Capacity = InternodeCapacity + NodeCapacity;

        [Test]
        public void EnergyShouldFlowUpBranches()
        {
            var bottom = CreateNode(Capacity, Entity.Null);
            var top = CreateNode(0, bottom);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(Capacity / 2, m_Manager.GetComponentData<EnergyStore>(top).Quantity, 0.001f);
            Assert.AreEqual(Capacity / 2, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity, 0.001f);
        }

        [Test]
        public void EnergyShouldFlowDownBranches()
        {
            var bottom = CreateNode(0, Entity.Null);
            var top = CreateNode(Capacity, bottom);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(Capacity / 2, m_Manager.GetComponentData<EnergyStore>(top).Quantity, 0.001f);
            Assert.AreEqual(Capacity / 2, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity, 0.001f);
        }

        [TestCase(1)]
        [TestCase(5)]
        public void EnergyThroughputShouldBeRelativeToNumberOfConnections (int branches) 
        {
            var bottom = CreateNode(Capacity, Entity.Null);

            var nodes = new List<Entity>();

            for (int i = 0; i < branches; i++)
            {
                var top = CreateNode(0, bottom);
                nodes.Add(top);
            }

            World.CreateSystem<EnergyFlowSystem>().Update();

            foreach (var node in nodes)
            {
                Assert.AreEqual(Capacity / (branches + 1), m_Manager.GetComponentData<EnergyFlow>(node).Throughput, 0.001f);
            }
        }

        [Test]
        public void NodeQuantityShouldNotExceedNodeCapacity()
        {
            var bottom = CreateNode( 1000, Entity.Null);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(Capacity, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity, 0.001f);
        }

        [Test]
        public void CapacityGetsRecalculatedBasedOnNodeAndInternodeSize()
        {
            var bottom = CreateNode(0, Entity.Null);

            m_Manager.SetComponentData(bottom, new Internode() { Radius = 1, Length = 2});

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(2 * InternodeCapacity + NodeCapacity, m_Manager.GetComponentData<EnergyStore>(bottom).Capacity, 0.001f);
        }

        private Entity CreateNode(float quantity, Entity parent)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Parent { Value = parent });
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = Capacity, Quantity = quantity });
            m_Manager.AddComponentData(entity, new EnergyFlow());
            m_Manager.AddComponentData(entity, new Node{Size = new float3(1,1,1)});
            m_Manager.AddComponentData(entity, new Internode {Length = 1, Radius = 1});
            m_Manager.AddBuffer<Child>(entity);
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);

            if (parent != Entity.Null)
            {
                var children = m_Manager.GetBuffer<Child>(parent);
                children.Add(new Child { Value = entity });
            }
            return entity;
        }
    }
}
