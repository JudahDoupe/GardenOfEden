using System;
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
    public class EmbryoDispersalSystemTests : ECSTestsFixture
    {
        [SetUp]
        public void SetUp()
        {
            Singleton.LandService = new MockLandService();
        }

        [TestCase(0.5f, false)]
        [TestCase(1f, true)]
        public void EmbryoOnlyDispersesWhenItIsFullyGrown(float energyQuantity, bool hasDisconected)
        {
            var baseNode = CreateNode();
            var embryo = CreateEmbryoNode(baseNode, energyQuantity);

            World.CreateSystem<EmbryoDispersalSystem>().Update();

            Assert.AreEqual(!hasDisconected, m_Manager.HasComponent<Parent>(embryo));
        }

        private Entity CreateNode()
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 1, Quantity = 1 });
            return entity;
        }

        private Entity CreateEmbryoNode(Entity baseNode, float energyQuantity)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Dormant());
            m_Manager.AddComponentData(entity, new WindDispersal());
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new Parent { Value = baseNode });
            m_Manager.AddComponentData(entity, new LocalToParent());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 1, Quantity = energyQuantity });
            return entity;
        }
    }

    public class MockLandService : ILandService
    {
        public Vector3 ClampAboveTerrain(Vector3 location) => location;

        public Vector3 ClampToTerrain(Vector3 location) => location;

        public void ProcessDay()
        {
        }

        public float SampleRootDepth(Vector3 location) => 0;

        public float SampleSoilDepth(Vector3 location) => 0;

        public float SampleTerrainHeight(Vector3 location) => 0;

        public float SampleWaterDepth(Vector3 location) => 0;
    }
}
