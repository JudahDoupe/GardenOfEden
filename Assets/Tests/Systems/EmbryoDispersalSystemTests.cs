using Assets.Plants.Systems.Cleanup;
using Assets.Scripts.Plants.Growth;
using NUnit.Framework;
using Unity.Entities;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class EmbryoDispersalSystemTests : SystemTestBase
    {

        [TestCase(0.5f, false)]
        [TestCase(1f, true)]
        public void EmbryoOnlyDispersesWhenItIsFullyGrown(float energyQuantity, bool hasDisconnected)
        {
            var baseNode = CreateNode();
            var embryo = CreateEmbryoNode(baseNode, energyQuantity);

            World.CreateSystem<EmbryoDispersalSystem>().Update();
            World.GetExistingSystem<GrowthEcbSystem>().Update();

            Assert.AreEqual(!hasDisconnected, m_Manager.HasComponent<Parent>(embryo));
        }

        private Entity CreateNode()
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new Translation());
            m_Manager.AddComponentData(entity, new Rotation());
            m_Manager.AddComponentData(entity, new LocalToWorld());
            m_Manager.AddComponentData(entity, new EnergyStore { Capacity = 1, Quantity = 1 });
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
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
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
            return entity;
        }
    }
}
