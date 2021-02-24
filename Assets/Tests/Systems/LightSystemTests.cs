using Assets.Scripts.Plants.Environment;
using FsCheck;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class LightSystemTests : SystemTestBase
    {
        public static Gen<LightBlocker> GenLightBlocker() =>
            from sa in Arb.Generate<float>()
            from id in CoordinateTests.GenXyw(Coordinate.TextureWidthInPixels)
            where 0 < sa && sa < LightSystem.LightPerCell
            select new LightBlocker { SurfaceArea = sa, CellId = id };

        public static Gen<LightAbsorber> GenLightAbsorber() =>
            from al in Arb.Generate<float>()
            where 0 <= al && al <= LightSystem.LightPerCell
            select new LightAbsorber { AbsorbedLight = al };

        private static Gen<Translation> GenTranslation() =>
            from y in Arb.Generate<float>()
            where 500 < y && y < 5000
            select new Translation { Value = new float3(0, y, 0) };

        private static Gen<AbsorberData> GenAbsorberData() =>
            from lb in GenLightBlocker()
            from la in GenLightAbsorber()
            from t in GenTranslation()
            select new AbsorberData {
                lightBlocker = lb,
                lightAbsorber = la,
                translation = t
            };

        private static Arbitrary<AbsorberData[]> ArbAbsorberDataArray(int maxEntities = 10) =>
            (from n in Gen.Choose(1, maxEntities)
            from array in Gen.ArrayOf(n, GenAbsorberData())
            select array).ToArbitrary();

        [Test]
        public void AbsorbedLightShouldBeLessThanOrEqualToAvailableLight() 
        {
            Prop.ForAll(ArbAbsorberDataArray(10), data =>
            {
                RunSystems(data);

                var totalAbsorbedLight = 0f;
                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightAbsorber)).ToEntityArray(Allocator.Temp))
                {
                    var absorber = m_Manager.GetComponentData<LightAbsorber>(entity);
                    totalAbsorbedLight += absorber.AbsorbedLight;
                }

                Assert.LessOrEqual(totalAbsorbedLight, LightSystem.LightPerCell, $"AbsorbedLight: {totalAbsorbedLight}, AvailableLight: {LightSystem.LightPerCell}");

            }).Check(_config);
        }

        [Test]
        public void AbsorbedLightShouldBeRelativeToSurfaceArea() 
        {
            Prop.ForAll(ArbAbsorberDataArray(1), data =>
            {
                RunSystems(data);

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightAbsorber)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var absorber = m_Manager.GetComponentData<LightAbsorber>(entity);
                    Assert.AreEqual(absorber.AbsorbedLight, blocker.SurfaceArea, $"AbsorbedLight: {absorber.AbsorbedLight}, SurfaceArea: {blocker.SurfaceArea}");
                }
            }).Check(_config);
        }

        [Test]
        public void HeigherAbsorbersShouldReceiveLightBeforeLowerAbsorbers() 
        {
            Prop.ForAll(ArbAbsorberDataArray(10), data =>
            {
                RunSystems(data);

                var results = new List<Tuple<float, float, float>>();
                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightAbsorber)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var absorber = m_Manager.GetComponentData<LightAbsorber>(entity);
                    var translation = m_Manager.GetComponentData<Translation>(entity);
                    results.Add(Tuple.Create(translation.Value.y, absorber.AbsorbedLight, blocker.SurfaceArea));
                }
                var expected = results.OrderBy(x => x.Item1);
                var actual = expected.OrderBy(x => x.Item2 / x.Item3);

                return expected.SequenceEqual(actual);

            }).Check(_config);
        }

        [Test]
        public void AbsorbedLightGetsReset() 
        {
            Prop.ForAll(ArbAbsorberDataArray(1), data =>
            {
                foreach (var item in data)
                {
                    item.lightAbsorber.AbsorbedLight = -1;
                }

                RunSystems(data);

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightAbsorber)).ToEntityArray(Allocator.Temp))
                {
                    var absorber = m_Manager.GetComponentData<LightAbsorber>(entity);
                    Assert.AreNotEqual(-1, absorber.AbsorbedLight);
                }
            }).Check(_config);
        }

        [Test]
        public void CellIdGetsUpdated() 
        {
            var cellId = new Coordinate(new float3(0,500,0)).xyw;

            Prop.ForAll(ArbAbsorberDataArray(1), data =>
            {
                RunSystems(data);

                foreach(var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var translation = m_Manager.GetComponentData<Translation>(entity);
                    Assert.AreEqual(cellId, blocker.CellId, $"Translation {translation.Value}");
                }
            }).Check(_config);
        }

        [Test]
        public void SurfaceAreaGetsUpdatedForNode()
        {
        }

        [Test]
        public void SurfaceAreaGetsUpdatedForInternode()
        {
        }

        [Test]
        public void SurfaceAreaIncludesNodeAndInternode()
        {
        }

        [Test]
        public void SurfaceAreaDoesNotGetUpdatedForEntitiesWithoutNodesOrInternodes()
        {
        }

        private void RunSystems(AbsorberData[] data)
        {
            m_Manager.DestroyAndResetAllEntities();

            foreach(var absorber in data)
            {
                var entity = m_Manager.CreateEntity();
                m_Manager.AddComponentData(entity, absorber.lightAbsorber);
                m_Manager.AddComponentData(entity, absorber.lightBlocker);
                m_Manager.AddComponentData(entity, absorber.translation);
                m_Manager.AddComponent(entity, typeof(LocalToWorld));
                m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
            }

            World.GetOrCreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
            World.GetOrCreateSystem<LightSystem>().Update();
        }

        private class AbsorberData
        {
            public LightBlocker lightBlocker;
            public LightAbsorber lightAbsorber;
            public Translation translation;
        }
    }
}
