using Assets.Scripts.Plants.Systems;
using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class GrowthSystemTests : ECSTestsFixture
    {
        [Test]
        public void GrowsNodeByGrowthRate()
        {
            var node = CreateNode(100, true, false);
            var oldVolume = m_Manager.GetComponentData<Node>(node).Volume;
            var oldSize = m_Manager.GetComponentData<Node>(node).Size;
            Assert.AreEqual(new float3(0,0,0), oldSize);

            World.CreateSystem<GrowthSystem>().Update();

            var newVolume = m_Manager.GetComponentData<Node>(node).Volume;
            var newSize = m_Manager.GetComponentData<Node>(node).Size;
            var growth = newVolume - oldVolume;
            var energyCost = growth / 4;
            Assert.AreEqual(new float3(1,1,1), newSize);
            Assert.AreEqual(100 - energyCost, m_Manager.GetComponentData<EnergyStore>(node).Quantity);
        }

        [Test]
        public void OnlyUsesHalfItsEnergyStoreToGrowNode()
        {
            var node = CreateNode(0.25f, true, false);
            Assert.AreEqual(0.25f, m_Manager.GetComponentData<EnergyStore>(node).Quantity);
            Assert.AreEqual(0, m_Manager.GetComponentData<Node>(node).Volume, 0.001f);

            World.CreateSystem<GrowthSystem>().Update();

            Assert.AreEqual(0.125f, m_Manager.GetComponentData<EnergyStore>(node).Quantity);
            Assert.AreEqual(0.5f, m_Manager.GetComponentData<Node>(node).Volume, 0.001f);
        }

        [Test]
        public void DoesNotGrowNodePastMaxSize()
        {
            var node = CreateNode(100, true, false);
            var volume1 = m_Manager.GetComponentData<Node>(node).Volume;
            var size1 = m_Manager.GetComponentData<Node>(node).Size;
            Assert.AreEqual(new float3(0,0,0), size1);

            World.CreateSystem<GrowthSystem>().Update();

            var volume2 = m_Manager.GetComponentData<Node>(node).Volume;
            var size2 = m_Manager.GetComponentData<Node>(node).Size;
            var growth = volume2 - volume1;
            var energyCost = growth / 4;
            Assert.AreEqual(new float3(1, 1, 1), size2);
            Assert.AreEqual(100 - energyCost, m_Manager.GetComponentData<EnergyStore>(node).Quantity);

            World.CreateSystem<GrowthSystem>().Update();

            var size3 = m_Manager.GetComponentData<Node>(node).Size;
            Assert.AreEqual(new float3(1, 1, 1), size3);
            Assert.AreEqual(100 - energyCost, m_Manager.GetComponentData<EnergyStore>(node).Quantity);
        }

        [Test]
        public void GrowsInterodeByGrowthRate()
        {
            var node = CreateNode(100, false, true);
            var oldVolume = m_Manager.GetComponentData<Internode>(node).Volume;
            var l1 = m_Manager.GetComponentData<Internode>(node).Length;
            var r1 = m_Manager.GetComponentData<Internode>(node).Radius;
            Assert.AreEqual(0, l1);
            Assert.AreEqual(0, r1);

            World.CreateSystem<GrowthSystem>().Update();

            var newVolume = m_Manager.GetComponentData<Internode>(node).Volume;
            var l2 = m_Manager.GetComponentData<Internode>(node).Length;
            var r2 = m_Manager.GetComponentData<Internode>(node).Radius;
            var growth = newVolume - oldVolume;
            var energyCost = growth / 4;
            Assert.AreEqual(1, l2);
            Assert.AreEqual(1, r2);
            Assert.AreEqual(100 - energyCost, m_Manager.GetComponentData<EnergyStore>(node).Quantity);
        }

        [Test]
        public void OnlyUsesHalfItsEnergyStoreToGrowInternode()
        {
            var node = CreateNode(0.25f, false, true);
            Assert.AreEqual(0, m_Manager.GetComponentData<Internode>(node).Volume, 0.001f);
            Assert.AreEqual(0.25f, m_Manager.GetComponentData<EnergyStore>(node).Quantity, 0.001f);

            World.CreateSystem<GrowthSystem>().Update();

            Assert.AreEqual(0.5f, m_Manager.GetComponentData<Internode>(node).Volume, 0.001f);
            Assert.AreEqual(0.125f, m_Manager.GetComponentData<EnergyStore>(node).Quantity, 0.001f);
        }

        [Test]
        public void DoesNotGrowInterodePastMaxSize()
        {
            var node = CreateNode(100, false, true);
            var volume1 = m_Manager.GetComponentData<Internode>(node).Volume;
            var l1 = m_Manager.GetComponentData<Internode>(node).Length;
            var r1 = m_Manager.GetComponentData<Internode>(node).Radius;
            Assert.AreEqual(0, l1);
            Assert.AreEqual(0, r1);

            World.CreateSystem<GrowthSystem>().Update();

            var volume2 = m_Manager.GetComponentData<Internode>(node).Volume;
            var l2 = m_Manager.GetComponentData<Internode>(node).Length;
            var r2 = m_Manager.GetComponentData<Internode>(node).Radius;
            var growth2 = volume2 - volume1;
            var energyCost2 = growth2 / 4;
            Assert.AreEqual(1, l2);
            Assert.AreEqual(1, r2);
            Assert.AreEqual(100 - energyCost2, m_Manager.GetComponentData<EnergyStore>(node).Quantity, 0.001f);

            World.CreateSystem<GrowthSystem>().Update();

            var volume3 = m_Manager.GetComponentData<Internode>(node).Volume;
            var l3 = m_Manager.GetComponentData<Internode>(node).Length;
            var r3 = m_Manager.GetComponentData<Internode>(node).Radius;
            var growth3 = volume3 - volume2;
            var energyCost3 = growth3 / 4;
            Assert.AreEqual(2, l3);
            Assert.AreEqual(1, r3);
            Assert.AreEqual(100 - energyCost2 - energyCost3, m_Manager.GetComponentData<EnergyStore>(node).Quantity, 0.001f);

            World.CreateSystem<GrowthSystem>().Update();

            var l4 = m_Manager.GetComponentData<Internode>(node).Length;
            var r4 = m_Manager.GetComponentData<Internode>(node).Radius;
            Assert.AreEqual(2, l4);
            Assert.AreEqual(1, r4);
            Assert.AreEqual(100 - energyCost2 - energyCost3, m_Manager.GetComponentData<EnergyStore>(node).Quantity, 0.001f);
        }

        [Test]
        public void GrowingInternodeMovesNode()
        {
            var node = CreateNode(100, false, true);
            Assert.AreEqual(0, m_Manager.GetComponentData<Translation>(node).Value.z);

            World.CreateSystem<GrowthSystem>().Update();

            Assert.AreEqual(1, m_Manager.GetComponentData<Translation>(node).Value.z);
        }

        [Test]
        public void GrowsNodeAndInternode()
        {
            var node = CreateNode(100, true, true);
            var oldNodeVolume = m_Manager.GetComponentData<Node>(node).Volume;
            var oldInternodeVolume = m_Manager.GetComponentData<Internode>(node).Volume;
            var l1 = m_Manager.GetComponentData<Internode>(node).Length;
            var r1 = m_Manager.GetComponentData<Internode>(node).Radius;
            var oldSize = m_Manager.GetComponentData<Node>(node).Size;
            Assert.AreEqual(new float3(0, 0, 0), oldSize);
            Assert.AreEqual(0, l1);
            Assert.AreEqual(0, r1);

            World.CreateSystem<GrowthSystem>().Update();

            var newNodeVolume = m_Manager.GetComponentData<Node>(node).Volume;
            var newSize = m_Manager.GetComponentData<Node>(node).Size;
            var newInternodeVolume = m_Manager.GetComponentData<Internode>(node).Volume;
            var l2 = m_Manager.GetComponentData<Internode>(node).Length;
            var r2 = m_Manager.GetComponentData<Internode>(node).Radius;
            var growth = (newNodeVolume + newInternodeVolume) - (oldNodeVolume + oldInternodeVolume);
            var energyCost = growth / 4;
            Assert.AreEqual(new float3(1,1,1), newSize);
            Assert.AreEqual(1, l2);
            Assert.AreEqual(1, r2);
            Assert.AreEqual(100 - energyCost, m_Manager.GetComponentData<EnergyStore>(node).Quantity);
        }

        private Entity CreateNode(float energy, bool includeNode, bool includeInternode)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new PrimaryGrowth {GrowthRate = 1, InternodeLength = 2, InternodeRadius = 1, NodeSize = new float3(1,1,1) });
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 100, Quantity = energy });

            if (includeInternode)
            {
                m_Manager.AddComponentData(entity, new Internode());
            }
            if (includeNode)
            {
                m_Manager.AddComponentData(entity, new Node());
            }
            return entity;
        }
    }
}
