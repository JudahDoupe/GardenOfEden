using System.Collections.Generic;
using Assets.Scripts.Plants.ECS.Components;
using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;
using Assets.Scripts.Plants.ECS.Services.TransportationSystems;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    public class EnergyFlowSystemTests : ECSTestsFixture
    {
        [Test]
        public void EnergyShouldFlowUpBranches()
        {
            var plant = CreatePlant();
            var bottom = CreateNode(1, 1, plant);
            var top = CreateNode(1, 0, bottom);
            CreateInternode(bottom, plant, false);
            CreateInternode(top, bottom);
            m_Manager.RemoveComponent<Child>(top);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(0.5f, m_Manager.GetComponentData<EnergyStore>(top).Quantity);
            Assert.AreEqual(0.5f, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity);
        }

        [Test]
        public void EnergyShouldFlowDownBranches()
        {
            var plant = CreatePlant();
            var bottom = CreateNode(1, 0, plant);
            var top = CreateNode(1, 1, bottom);
            CreateInternode(bottom, plant, false);
            CreateInternode(top, bottom);
            m_Manager.RemoveComponent<Child>(top);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(0.5f, m_Manager.GetComponentData<EnergyStore>(top).Quantity);
            Assert.AreEqual(0.5f, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity);
        }

        [TestCase(1)]
        [TestCase(5)]
        public void EnergyThroughputShouldBeRelativeToNumberOfConnections (int branches) 
        {
            var plant = CreatePlant();
            var bottom = CreateNode(1, 1, plant);
            CreateInternode(bottom, plant, false);

            var internodes = new List<Entity>();
            var nodes = new List<Entity>();

            for (int i = 0; i < branches; i++)
            {
                var top = CreateNode(1, 0, bottom);
                nodes.Add(top);
                internodes.Add(CreateInternode(top, bottom));
            }

            World.CreateSystem<EnergyFlowSystem>().Update();

            foreach (var connection in internodes)
            {
                Assert.AreEqual(1f / (branches + 1), m_Manager.GetComponentData<EnergyFlow>(connection).Throughput);
            }
        }

        [Test]
        public void NodeQuantityShouldNotExceedNodeCapacityPlusInternodeCapacity()
        {
            var plant = CreatePlant();
            var bottom = CreateNode(1, 100, plant);
            CreateInternode(bottom, plant, false, 1, 1);

            World.CreateSystem<EnergyFlowSystem>().Update();

            var internodeCapacity = math.PI * 0.3f;

            Assert.AreEqual(1 + internodeCapacity, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity);
        }


        private Entity CreatePlant()
        {
            var entity = m_Manager.CreateEntity(
                typeof(Translation),
                typeof(Rotation));
            m_Manager.AddBuffer<Child>(entity);
            return entity;
        }

        private Entity CreateNode(float capacity, float quantity, Entity parent)
        {
            var entity = m_Manager.CreateEntity(
                typeof(EnergyStore),
                typeof(InternodeReference),
                typeof(Translation),
                typeof(Rotation),
                typeof(Parent));
            m_Manager.SetComponentData(entity, new Parent { Value = parent });
            m_Manager.SetComponentData(entity, new EnergyStore { Capacity = capacity, Quantity = quantity });
            m_Manager.AddBuffer<Child>(entity);
            return entity;
        }

        private Entity CreateInternode(Entity head, Entity tail, bool addFlow = true, float length = 0, float radius = 0)
        {
            var entity = m_Manager.CreateEntity(
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(Internode));
            m_Manager.SetComponentData(entity, new Internode { HeadNode = head, TailNode = tail, Length = length, Radius = radius});
            m_Manager.SetComponentData(head, new InternodeReference {Internode = entity});

            var children = m_Manager.GetBuffer<Child>(tail);
            children.Add(new Child {Value = head});


            if (addFlow) 
            {
                m_Manager.AddComponentData(entity, new EnergyFlow());
            }

            return entity;
        }
    }
}
