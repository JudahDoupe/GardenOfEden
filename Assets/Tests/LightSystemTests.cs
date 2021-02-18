using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class LightSystemTests : SystemTestBase
    {
        [Test]
        public void LightAbsorbersShouldBlockLight()
        {
            var top = CreateNode(10, true, false);
            m_Manager.SetComponentData(top, new Node { Size = new float3(4, 0.1f, 4) });
            var bottom = CreateNode(5, true, false);
            m_Manager.SetComponentData(bottom, new Node { Size = new float3(4, 0.1f, 4) });
            m_Manager.AddComponentData(bottom, new Photosynthesis { Efficiency = 1 });

            World.CreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
            World.CreateSystem<LightSystem>().Update();

            Assert.AreEqual(16, m_Manager.GetComponentData<EnergyStore>(top).Quantity, 0.001f);
            Assert.AreEqual(9, m_Manager.GetComponentData<EnergyStore>(bottom).Quantity, 0.001f);
        }

        [Test]
        public void PhotosynthesisShouldTurnLightIntoEnergy()
        {
            var top = CreateNode(10, true, false);
            m_Manager.SetComponentData(top, new Node { Size = new float3(5, 0.1f, 5) });

            World.CreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
            World.CreateSystem<LightSystem>().Update();

            Assert.AreEqual(25, m_Manager.GetComponentData<EnergyStore>(top).Quantity, 0.001f);
        }

        [TestCase(0,0,0,1, 2)]
        [TestCase(0.7071068f, 0, 0, 0.7071068f, 0.1f)]
        [TestCase(0, 0, 0.7071068f, 0.7071068f, 0.2f)]
        [TestCase(0, 0, 0.3826834f, 0.9238795f, 1.2728f)]
        public void NodeSurfaceAreaTests(float x, float y, float z, float w, float result)
        {
            var node = CreateNode(10, true, false);
            m_Manager.SetComponentData(node, new Rotation { Value = new quaternion(x,y,z,w) });

            World.CreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
            World.CreateSystem<LightSystem>().Update();

            Assert.AreEqual(result, m_Manager.GetComponentData<LightBlocker>(node).SurfaceArea, 0.001f);
        }

        [TestCase(0, 0, 0, 1, 1)]
        [TestCase(0.7071068f, 0, 0, 0.7071068f, 0.25f)]
        [TestCase(0, 0, 0.7071068f, 0.7071068f, 1)]
        [TestCase(0.3826834f, 0, 0, 0.9238795f, 0.88388f)]
        public void InternodeSurfaceAreaTests(float x, float y, float z, float w, float result)
        {
            var node = CreateNode(10, false, true);
            m_Manager.SetComponentData(node, new Rotation { Value = new quaternion(x, y, z, w) });

            World.CreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
            World.CreateSystem<LightSystem>().Update();

            Assert.AreEqual(result, m_Manager.GetComponentData<LightBlocker>(node).SurfaceArea, 0.001f);
        }

        [TestCase(0, 0, 0, 1, 3)]
        [TestCase(0.7071068f, 0, 0, 0.7071068f, 0.35f)]
        [TestCase(0, 0, 0.7071068f, 0.7071068f, 1.2f)]
        [TestCase(0.3826834f, 0, 0, 0.9238795f, 2.3688f)]
        public void NodeAndInternodeSurfaceAreaTests(float x, float y, float z, float w, float result)
        {
            var node = CreateNode(10, true, true);
            m_Manager.SetComponentData(node, new Rotation { Value = new quaternion(x, y, z, w) });

            World.CreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
            World.CreateSystem<LightSystem>().Update();

            Assert.AreEqual(result, m_Manager.GetComponentData<LightBlocker>(node).SurfaceArea, 0.001f);
        }

        private Entity CreateNode(float height, bool includeNode, bool includeInternode)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Translation {Value = new float3(0, height, 0) });
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new LightAbsorber());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 100 });
            m_Manager.AddComponentData(entity, new Photosynthesis { Efficiency = 1 });
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);

            if (includeInternode)
            {
                m_Manager.AddComponentData(entity, new Internode { Length = 2, Radius = 0.5f}) ;
            }
            if (includeNode)
            {
                m_Manager.AddComponentData(entity, new Node {Size = new float3(1,0.1f,2) });
            }
            return entity;
        }
    }
}
