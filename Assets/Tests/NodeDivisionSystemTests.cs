using Assets.Scripts.Plants.Systems;
using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Tests
{
    [Category("Systems")]
    public class NodeDivisionSystemTests : ECSTestsFixture
    {
        [TestCase(DivisionOrder.InPlace)]
        [TestCase(DivisionOrder.PreNode)]
        [TestCase(DivisionOrder.PostNode)]
        [TestCase(DivisionOrder.Replace)]
        public void ShouldReparentNodesCorrectly(DivisionOrder order)
        {
            var dna = CreateDna();
            var bottom = CreateNode(dna);
            var top = CreateNode(dna);
            m_Manager.SetComponentData(top, new Parent { Value = bottom });
            var embryo = CreateEmbryoNode(dna, order, NodeType.Vegetation);
            m_Manager.AddComponentData(top, new NodeDivision{Type = NodeType.Vegetation});

            World.CreateSystem<NodeDivisionSystem>().Update();
            World.CreateSystem<EndFrameParentSystem>().Update();

            switch (order)
            {
                case DivisionOrder.Replace:
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(bottom).Length);
                    break;
                case DivisionOrder.InPlace:
                    Assert.AreEqual(2, m_Manager.GetBuffer<Child>(bottom).Length);
                    break;
                case DivisionOrder.PreNode:
                    var children = m_Manager.GetBuffer<Child>(bottom);
                    Assert.AreEqual(1, children.Length);
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(children.ElementAt(0).Value).Length);
                    break;
                case DivisionOrder.PostNode:
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(bottom).Length);
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(top).Length);
                    break;
            }
        }

        [TestCase(0.4f)]
        [TestCase(0.6f)]
        public void ShouldOnlyDivideWhenBudHasEnoughEnergy(float quantity)
        {
            var dna = CreateDna();
            var bottom = CreateNode(dna);
            var top = CreateNode(dna);
            m_Manager.SetComponentData(top, new Parent { Value = bottom });
            m_Manager.SetComponentData(top, new EnergyStore { Capacity = 1, Quantity = quantity });
            var embryo = CreateEmbryoNode(dna, DivisionOrder.InPlace, NodeType.Vegetation);
            m_Manager.AddComponentData(top, new NodeDivision { Type = NodeType.Vegetation });

            World.CreateSystem<NodeDivisionSystem>().Update();
            World.CreateSystem<EndFrameParentSystem>().Update();

            Assert.AreEqual(quantity > 0.5f ? 2 : 1, m_Manager.GetBuffer<Child>(bottom).Length);
        }

        [TestCase(0)]
        [TestCase(5)]
        public void OnlyDividesNodeASetNumberOfTimes(int divisions)
        {
            var dna = CreateDna();
            var baseNode = CreateNode(dna);
            var embryo = CreateEmbryoNode(dna, DivisionOrder.PostNode, NodeType.Vegetation);
            m_Manager.AddComponentData(baseNode, new NodeDivision { RemainingDivisions = divisions, Type = NodeType.Vegetation });

            for (int i = 0; i < divisions + 5; i++)
            {
                World.CreateSystem<NodeDivisionSystem>().Update();
                World.CreateSystem<EndFrameParentSystem>().Update();
            }

            Assert.AreEqual(divisions + 1, m_Manager.GetBuffer<Child>(baseNode).Length);
        }

        [Test]
        public void UnsetRemainingDivisionsDividesOnce()
        {
            var dna = CreateDna();
            var baseNode = CreateNode(dna);
            var embryo = CreateEmbryoNode(dna, DivisionOrder.PostNode, NodeType.Vegetation);
            m_Manager.AddComponentData(baseNode, new NodeDivision { Type = NodeType.Vegetation });

            for (int i = 0; i <  5; i++)
            {
                World.CreateSystem<NodeDivisionSystem>().Update();
                World.CreateSystem<EndFrameParentSystem>().Update();
            }

            Assert.AreEqual( 1, m_Manager.GetBuffer<Child>(baseNode).Length);
        }

        [Test]
        public void EmbryoNodesRemainDormantAfterDivision()
        {
            var dna = CreateDna();
            var baseNode = CreateNode(dna);
            var embryo = CreateEmbryoNode(dna, DivisionOrder.PostNode, NodeType.Embryo);
            m_Manager.AddComponentData(baseNode, new NodeDivision { Type = NodeType.Embryo });

            for (int i = 0; i < 5; i++)
            {
                World.CreateSystem<NodeDivisionSystem>().Update();
                World.CreateSystem<EndFrameParentSystem>().Update();
            }

            Assert.AreEqual(1, m_Manager.GetBuffer<Child>(baseNode).Length);
            Assert.True(m_Manager.HasComponent<Dormant>(m_Manager.GetBuffer<Child>(baseNode)[0].Value));
        }

        private Entity CreateNode(Entity dna)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Node { Size = new float3(0.5f, 0.5f, 0.5f) });
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new Parent());
            m_Manager.AddComponentData(entity, new LocalToParent());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 1, Quantity = 1 });
            m_Manager.AddComponentData(entity, new EnergyFlow());
            m_Manager.AddComponentData(entity, new DnaReference { Entity = dna});
            return entity;
        }

        private Entity CreateEmbryoNode(Entity dna, DivisionOrder order, NodeType type)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Dormant());
            m_Manager.AddComponentData(entity, new Node());
            m_Manager.AddComponentData(entity, new Internode());
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new Parent());
            m_Manager.AddComponentData(entity, new LocalToParent());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 1, Quantity = 1 });
            m_Manager.AddComponentData(entity, new EnergyFlow());
            m_Manager.AddComponentData(entity, new DnaReference { Entity = dna });
            m_Manager.GetBuffer<EmbryoNode>(dna).Add(new EmbryoNode{Entity = entity, Order = order, Type = type});
            return entity;
        }

        private Entity CreateDna()
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddBuffer<EmbryoNode>(entity);
            return entity;
        }
    }
}
