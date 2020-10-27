using System.Linq;
using Assets.Scripts.Plants.ECS.Components;
using Assets.Scripts.Plants.ECS.Services;
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
        public void ShouldReparentNodesCorrectly(DivisionOrder order)
        {
            var bottom = CreateNode();
            var top = CreateNode();
            m_Manager.SetComponentData(top, new Parent { Value = bottom });
            var embryo = CreateEmbryoNode();
            var embryoBuffer = m_Manager.AddBuffer<NodeDivision>(top);
            embryoBuffer.Add(new NodeDivision { Entity = embryo, Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right), Order = order, NumDivisions = -1 });

            World.CreateSystem<NodeDivisionSystem>().Update();
            World.CreateSystem<EndFrameParentSystem>().Update();

            switch (order)
            {
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
            var bottom = CreateNode();
            var top = CreateNode();
            m_Manager.SetComponentData(top, new Parent { Value = bottom });
            m_Manager.SetComponentData(top, new EnergyStore { Capacity = 1, Quantity = quantity });
            var embryo = CreateEmbryoNode();
            var embryoBuffer = m_Manager.AddBuffer<NodeDivision>(top);
            embryoBuffer.Add(new NodeDivision { Entity = embryo, Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right), Order = DivisionOrder.InPlace, NumDivisions = -1});

            World.CreateSystem<NodeDivisionSystem>().Update();
            World.CreateSystem<EndFrameParentSystem>().Update();

            Assert.AreEqual(quantity > 0.5f ? 2 : 1, m_Manager.GetBuffer<Child>(bottom).Length);
        }

        [TestCase(1)]
        [TestCase(5)]
        public void OnlyDividesNodeASetNumberOfTimes(int divisions)
        {

            var baseNode = CreateNode();
            var embryo = CreateEmbryoNode();
            var embryoBuffer = m_Manager.AddBuffer<NodeDivision>(baseNode);
            embryoBuffer.Add(new NodeDivision { Entity = embryo, Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right), Order = DivisionOrder.PostNode, NumDivisions = divisions });

            for (int i = 0; i < divisions + 2; i++)
            {
                World.CreateSystem<NodeDivisionSystem>().Update();
                World.CreateSystem<EndFrameParentSystem>().Update();
            }

            Assert.AreEqual(divisions, m_Manager.GetBuffer<Child>(baseNode).Length);
            Assert.AreEqual(0, m_Manager.GetBuffer<NodeDivision>(baseNode).Length);
        }

        private Entity CreateNode()
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Assets.Scripts.Plants.ECS.Components.Node { Size = new float3(0.5f, 0.5f, 0.5f) });
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new Parent());
            m_Manager.AddComponentData(entity, new LocalToParent());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 1, Quantity = 1 });
            m_Manager.AddComponentData(entity, new EnergyFlow());
            return entity;
        }

        private Entity CreateEmbryoNode()
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Dormant());
            m_Manager.AddComponentData(entity, new Assets.Scripts.Plants.ECS.Components.Node());
            m_Manager.AddComponentData(entity, new Internode());
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new Parent());
            m_Manager.AddComponentData(entity, new LocalToParent());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 1, Quantity = 1 });
            m_Manager.AddComponentData(entity, new EnergyFlow());
            return entity;
        }
    }
}
