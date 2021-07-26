using Assets.Scripts.Plants.Growth;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class NodeDivisionSystemTests : SystemTestBase
    {
        [TestCase(DivisionOrder.InPlace)]
        [TestCase(DivisionOrder.PreNode)]
        [TestCase(DivisionOrder.PostNode)]
        [TestCase(DivisionOrder.Replace)]
        public void ShouldReparentNodesCorrectly(DivisionOrder order)
        {
            var bottom = CreateNode();
            var middle = CreateNode();
            var top = CreateNode();
            m_Manager.SetComponentData(top, new Parent { Value = middle });
            m_Manager.SetComponentData(middle, new Parent { Value = bottom });

            var embryo = CreateNode();
            m_Manager.AddComponentData(middle, new NodeDivision());
            var buffer = m_Manager.AddBuffer<DivisionInstruction>(middle);
            buffer.Add(new DivisionInstruction { Entity = embryo, Order = order });

            World.GetOrCreateSystem<EndFrameParentSystem>().Update();
            World.GetOrCreateSystem<NodeDivisionSystem>().Update();
            World.GetOrCreateSystem<GrowthEcbSystem>().Update();
            World.GetOrCreateSystem<EndFrameParentSystem>().Update();

            Assert.IsTrue(m_Manager.HasComponent<Parent>(top));

            switch (order)
            {
                case DivisionOrder.Replace:
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(bottom).Length);
                    var newNode = m_Manager.GetBuffer<Child>(bottom).ElementAt(0).Value;
                    Assert.AreEqual(newNode, m_Manager.GetComponentData<Parent>(top).Value);
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(newNode).Length);
                    break;
                case DivisionOrder.InPlace:
                    Assert.AreEqual(2, m_Manager.GetBuffer<Child>(bottom).Length);
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(middle).Length);
                    Assert.AreEqual(middle, m_Manager.GetComponentData<Parent>(top).Value);
                    break;
                case DivisionOrder.PreNode:
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(bottom).Length);
                    newNode = m_Manager.GetBuffer<Child>(bottom).ElementAt(0).Value;
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(newNode).Length);
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(middle).Length);
                    Assert.AreEqual(middle, m_Manager.GetBuffer<Child>(newNode).ElementAt(0).Value);
                    Assert.AreEqual(middle, m_Manager.GetComponentData<Parent>(top).Value);
                    break;
                case DivisionOrder.PostNode:
                    Assert.AreEqual(1, m_Manager.GetBuffer<Child>(bottom).Length);
                    Assert.AreEqual(2, m_Manager.GetBuffer<Child>(middle).Length);
                    Assert.AreEqual(middle, m_Manager.GetComponentData<Parent>(top).Value);
                    break;
            }
        }

        [Test]
        public void ReparentTest()
        {
            var arch = m_Manager.CreateArchetype(
                typeof(Translation), typeof(Rotation), typeof(Parent), typeof(LocalToParent), typeof(LocalToWorld));

            var bottom = m_Manager.CreateEntity(arch);
            var middle = m_Manager.CreateEntity(arch);
            var top = m_Manager.CreateEntity(arch);
            m_Manager.SetComponentData(top, new Parent { Value = middle });
            m_Manager.SetComponentData(middle, new Parent { Value = bottom });

            World.GetOrCreateSystem<EndFrameParentSystem>().Update();
            var ecb = World.GetOrCreateSystem<GrowthEcbSystem>().CreateCommandBuffer();

            ecb.RemoveComponent<Child>(middle);
            ecb.DestroyEntity(middle);
            ecb.SetComponent(top, new Parent{Value = bottom});
            ecb.SetComponent(top, new PreviousParent{Value = Entity.Null});

            World.GetOrCreateSystem<GrowthEcbSystem>().Update();
            World.GetOrCreateSystem<EndFrameParentSystem>().Update();

            Assert.AreEqual(m_Manager.GetComponentData<Parent>(top).Value, bottom);
        }

        [TestCase(0.4f)]
        [TestCase(0.6f)]
        public void ShouldOnlyDivideWhenBudHasEnoughEnergy(float quantity)
        {
            var bottom = CreateNode();
            var top = CreateNode();
            m_Manager.SetComponentData(top, new Parent { Value = bottom });
            m_Manager.SetComponentData(top, new EnergyStore { Capacity = 1, Quantity = quantity });
            var embryo = CreateNode();
            m_Manager.AddComponentData(top, new NodeDivision { MinEnergyPressure = 0.5f });
            var buffer = m_Manager.AddBuffer<DivisionInstruction>(top);
            buffer.Add(new DivisionInstruction { Entity = embryo, Order = DivisionOrder.InPlace });

            World.GetOrCreateSystem<EndFrameParentSystem>().Update();
            Assert.AreEqual(1, m_Manager.GetBuffer<Child>(bottom).Length);

            World.GetOrCreateSystem<NodeDivisionSystem>().Update();
            World.GetOrCreateSystem<GrowthEcbSystem>().Update();
            World.GetOrCreateSystem<EndFrameParentSystem>().Update();

            Assert.AreEqual(quantity > 0.5f ? 2 : 1, m_Manager.GetBuffer<Child>(bottom).Length);
        }

        [TestCase(0)]
        [TestCase(5)]
        public void OnlyDividesNodeASetNumberOfTimes(int divisions)
        {

            var baseNode = CreateNode();
            var embryo = CreateNode();
            m_Manager.AddComponentData(baseNode, new NodeDivision { RemainingDivisions = divisions });
            var buffer = m_Manager.AddBuffer<DivisionInstruction>(baseNode);
            buffer.Add(new DivisionInstruction { Entity = embryo, Order = DivisionOrder.PostNode });

            for (int i = 0; i < divisions + 5; i++)
            {
                World.GetOrCreateSystem<NodeDivisionSystem>().Update();
                World.GetOrCreateSystem<GrowthEcbSystem>().Update();
                World.GetOrCreateSystem<EndFrameParentSystem>().Update();
            }

            Assert.AreEqual(divisions + 1, m_Manager.GetBuffer<Child>(baseNode).Length);
        }

        [Test]
        public void UnsetRemainingDivisionsDividesOnce()
        {
            var baseNode = CreateNode();
            var embryo = CreateNode();
            m_Manager.AddComponentData(baseNode, new NodeDivision { Stage = LifeStage.Vegetation });
            var buffer = m_Manager.AddBuffer<DivisionInstruction>(baseNode);
            buffer.Add(new DivisionInstruction { Entity = embryo, Order = DivisionOrder.PostNode, Stage = LifeStage.Vegetation });

            for (int i = 0; i <  5; i++)
            {
                World.GetOrCreateSystem<NodeDivisionSystem>().Update();
                World.GetOrCreateSystem<GrowthEcbSystem>().Update();
                World.GetOrCreateSystem<EndFrameParentSystem>().Update();
            }

            Assert.AreEqual( 1, m_Manager.GetBuffer<Child>(baseNode).Length);
        }

        private Entity CreateNode()
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
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
            return entity;
        }
    }
}
