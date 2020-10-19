using System.Collections.Generic;
using Assets.Scripts.Plants.ECS.Components;
using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;
using Assets.Scripts.Plants.ECS.Services.TransportationSystems;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Tests
{
    public class EnergyFlowSystemTests : ECSTestsFixture
    {
        const float InternodeCapacity = 3.1415f;
        const float NodeCapacity = 4.187743f;

        [Test]
        public void EnergyShouldFlowUpBranches()
        {
            var plant = CreatePlant();
            var bottom = CreateNode(InternodeCapacity, InternodeCapacity, plant);
            var top = CreateNode(InternodeCapacity, 0, bottom);
            CreateInternode(bottom, plant, false);
            CreateInternode(top, bottom, true);
            m_Manager.RemoveComponent<Child>(top);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(InternodeCapacity / 2, m_Manager.GetComponentData<EnergyStore>(top).Quantity, 0.0001f);
            Assert.AreEqual(InternodeCapacity / 2, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity, 0.0001f);
        }

        [Test]
        public void EnergyShouldFlowDownBranches()
        {
            var plant = CreatePlant();
            var bottom = CreateNode(InternodeCapacity, 0, plant);
            var top = CreateNode(InternodeCapacity, InternodeCapacity, bottom);
            CreateInternode(bottom, plant, false);
            CreateInternode(top, bottom, true);
            m_Manager.RemoveComponent<Child>(top);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(InternodeCapacity / 2, m_Manager.GetComponentData<EnergyStore>(top).Quantity, 0.0001f);
            Assert.AreEqual(InternodeCapacity / 2, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity, 0.0001f);
        }

        [TestCase(1)]
        [TestCase(5)]
        public void EnergyThroughputShouldBeRelativeToNumberOfConnections (int branches) 
        {
            var plant = CreatePlant();
            var bottom = CreateNode(InternodeCapacity, InternodeCapacity, plant);
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
                Assert.AreEqual(InternodeCapacity / (branches + 1), m_Manager.GetComponentData<EnergyFlow>(connection).Throughput, 0.0001f);
            }
        }

        [Test]
        public void NodeQuantityShouldNotExceedNodeCapacity()
        {
            var plant = CreatePlant();
            var bottom = CreateNode(InternodeCapacity, 100, plant);
            CreateInternode(bottom, plant, false);

            World.CreateSystem<EnergyFlowSystem>().Update();

            Assert.AreEqual(3.1415f, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity, 0.0001f);
        }

        [Test]
        public void CapacityGetsRecalculatedBasedOnNodeAndInternodeSize()
        {
            var plant = CreatePlant();
            var bottom = CreateNode(NodeCapacity + InternodeCapacity, 0, plant);
            CreateInternode(bottom, plant, false);
            m_Manager.AddComponentData(bottom, new RenderBounds { Value = new AABB{Extents = new float3(1,1,1)} });

            World.CreateSystem<EnergyFlowSystem>().Update();


            Assert.AreEqual(NodeCapacity + InternodeCapacity, m_Manager.GetComponentData<EnergyStore>(bottom).Capacity, 0.0001f);
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
            var entity = m_Manager.CreateEntity(typeof(InternodeReference),
                typeof(Translation),
                typeof(Rotation));
            m_Manager.AddComponentData(entity, new Parent { Value = parent });
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = capacity, Quantity = quantity });
            m_Manager.AddComponentData(entity, new NonUniformScale { Value = new float3(1, 1, 1) });
            m_Manager.AddBuffer<Child>(entity);
            return entity;
        }

        private Entity CreateInternode(Entity head, Entity tail, bool addFlow = true)
        {
            var entity = m_Manager.CreateEntity(
                typeof(Translation),
                typeof(Rotation));
            m_Manager.AddComponentData(entity, new Internode { HeadNode = head, TailNode = tail, Length = 1, Radius = 1});
            m_Manager.AddComponentData(entity, new NonUniformScale { Value = new float3(1, 1, 1) });
            m_Manager.AddComponentData(head, new InternodeReference {Internode = entity});

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
