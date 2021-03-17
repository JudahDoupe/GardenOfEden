using Assets.Scripts.Plants.Growth;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Tests
{
    [Category("Systems")]
    public class MetabolismSystemTests : SystemTestBase
    {
        public static Gen<Metabolism> GenMetabolism() =>
            from resting in FsCheckUtils.GenFloat(0, 10)
            select new Metabolism { Resting = resting };

        public static Gen<Health> GenHealth() =>
            from value in FsCheckUtils.GenFloat(-0.1f, 1)
            select new Health { Value = value };

        private static Gen<EntityData> GenEntityData() =>
            from energyStore in EnergyFlowSystemTests.GenEnergyStore()
            from health in GenHealth()
            from metabolism in GenMetabolism()
            select new EntityData
            {
                EnergyStore = energyStore,
                Health = health,
                Metabolism = metabolism,
            };

        private static Gen<EntityData> GenFullEntityData() =>
            from energyStore in EnergyFlowSystemTests.GenEnergyStore()
            from health in GenHealth()
            from metabolism in GenMetabolism()
            from size in FsCheckUtils.GenFloat3(new float3(0,0,0), new float3(2,2,2))
            from length in FsCheckUtils.GenFloat(0, 2)
            from radius in FsCheckUtils.GenFloat(0, 1)
            select new EntityData
            {
                EnergyStore = energyStore,
                Health = health,
                Metabolism = metabolism,
                Node = new Node { Size = size, InternodeLength = length, InternodeRadius = radius }
            };

        [Test]
        public void MetabolismUsesEnergyBasedOntheirVolume()
        {
            Prop.ForAll(GenFullEntityData().ToArbitrary(), data =>
            {
                RunSystems(new[] { data });

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(Metabolism)).ToEntityArray(Allocator.Temp))
                {
                    var requiredEnergy = data.Node.Value.Volume * data.Metabolism.Resting;
                    var store = m_Manager.GetComponentData<EnergyStore>(entity);
                    if (requiredEnergy > data.EnergyStore.Quantity)
                    {
                        store.Quantity.Should().Be(0);
                        var health = m_Manager.GetComponentData<Health>(entity);
                        health.Value.Should().BeApproximately(data.Health.Value + data.EnergyStore.Quantity - requiredEnergy, 0.001f);
                    }
                    else
                    {
                        store.Quantity.Should().BeApproximately(data.EnergyStore.Quantity - requiredEnergy, 0.001f);
                    }
                }

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void StoredEnergyShouldNeverDropBelow0()
        {
            Prop.ForAll(GenFullEntityData().ToArbitrary(), data =>
            {
                RunSystems(new[] { data });

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(Metabolism)).ToEntityArray(Allocator.Temp))
                {
                    var store = m_Manager.GetComponentData<EnergyStore>(entity);
                    store.Capacity.Should().BeGreaterOrEqualTo(0);
                }

            }).Check(FsCheckUtils.Config);
        }

        private void RunSystems(EntityData[] array)
        {
            m_Manager.DestroyAndResetAllEntities();

            foreach(var data in array)
            { 
                var entity = m_Manager.CreateEntity();
                m_Manager.AddComponentData(entity, data.EnergyStore);
                m_Manager.AddComponentData(entity, data.Health);
                m_Manager.AddComponentData(entity, data.Metabolism);
                if (data.Node.HasValue) m_Manager.AddComponentData(entity, data.Node.Value);
                m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
            }
            
            World.GetOrCreateSystem<MetabolismSystem>().Update();
        }

        private class EntityData
        {
            public EnergyStore EnergyStore;
            public Health Health;
            public Metabolism Metabolism;
            public Node? Node;

            public string ToErrorString()
            {
                return $@"
EnergyStore Capacity: {EnergyStore.Capacity}
EnergyStore Quantity: {EnergyStore.Quantity}
Health: {Health.Value}
Metabolism: {Metabolism.Resting}
Internode length: {Node?.InternodeLength}
Internode radius: {Node?.InternodeRadius}
Node: {Node?.Size}
";
            }
        }
    }
}
