using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
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
            from t in FsCheckUtils.Gen0To1()
            from id in CoordinateTansformTests.GenXyw(Coordinate.TextureWidthInPixels)
            select new LightBlocker { SurfaceArea = math.lerp(0, LightSystem.LightPerCell, t), CellId = id };

        public static Gen<LightAbsorber> GenLightAbsorber() =>
            from t in FsCheckUtils.Gen0To1()
            select new LightAbsorber { AbsorbedLight = math.lerp(0, LightSystem.LightPerCell, t) };

        private static Gen<Translation> GenTranslation() =>
            from t in FsCheckUtils.Gen0To1()
            select new Translation { Value = new float3(0, math.lerp(500, 5000, t), 0) };
        
        private static Gen<Rotation> GenRotation() =>
            from x in FsCheckUtils.Gen0To1()
            from y in FsCheckUtils.Gen0To1()
            from z in FsCheckUtils.Gen0To1()
            select new Rotation { Value = quaternion.Euler(math.lerp(0,math.PI,x), math.lerp(0, math.PI, x), math.lerp(0, math.PI, x)) };

        private static Gen<AbsorberData> GenAbsorberData() =>
            from lb in GenLightBlocker()
            from la in GenLightAbsorber()
            from t in GenTranslation()
            from r in GenRotation()
            select new AbsorberData {
                lightBlocker = lb,
                lightAbsorber = la,
                translation = t,
                Rotation = r,
            };

        private static Arbitrary<AbsorberData[]> ArbAbsorberDataArray(int numEntites = 10) =>
            (from array in Gen.ArrayOf(numEntites, GenAbsorberData())
            select array).ToArbitrary();

        [Test]
        public void AbsorbedLightShouldBeLessThanOrEqualToAvailableLight() 
        {
            Prop.ForAll(ArbAbsorberDataArray(25), data =>
            {
                RunSystems(data);

                var totalAbsorbedLight = 0f;
                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightAbsorber)).ToEntityArray(Allocator.Temp))
                {
                    var absorber = m_Manager.GetComponentData<LightAbsorber>(entity);
                    totalAbsorbedLight += absorber.AbsorbedLight;
                }

                totalAbsorbedLight.Truncate(5).Should().BeLessOrEqualTo(LightSystem.LightPerCell.Truncate(5));

            }).Check(FsCheckUtils.Config);
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
                    absorber.AbsorbedLight.Should().Be(blocker.SurfaceArea);
                }
            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void HigherAbsorbersShouldReceiveLightBeforeLowerAbsorbers() 
        {
            Prop.ForAll(ArbAbsorberDataArray(25), data =>
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

                expected.SequenceEqual(actual).Should().BeTrue();

            }).Check(FsCheckUtils.Config);
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
                    absorber.AbsorbedLight.Should().NotBe(-1);
                }
            }).Check(FsCheckUtils.Config);
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
                    blocker.CellId.Should().Be(cellId, $"Translation {translation.Value}");
                }
            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void SurfaceAreaIsWithinRangeForNode()
        {
            Prop.ForAll(ArbAbsorberDataArray(1), data =>
            {
                data[0].lightBlocker.SurfaceArea = 0;
                data[0].Node = new Node { Size = new float3(1, 1, 1) };
                var maxArea = math.pow(math.sqrt(3), 2) / 2;

                RunSystems(data);

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var rotation = m_Manager.GetComponentData<Rotation>(entity);
                    blocker.SurfaceArea.Should().BeLessOrEqualTo(maxArea, $"rotation: {rotation.Value}");
                    blocker.SurfaceArea.Should().BeGreaterOrEqualTo(1, $"rotation: {rotation.Value}");
                }
            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void SurfaceAreaIsWithinRangeForInternode()
        {
            Prop.ForAll(ArbAbsorberDataArray(1), data =>
            {
                data[0].lightBlocker.SurfaceArea = 0;
                data[0].Internode = new Internode { Length = 1, Radius = 0.05f };
                data[0].Node = new Node { Size = new float3(0,0,0) };

                RunSystems(data);

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var rotation = m_Manager.GetComponentData<Rotation>(entity);
                    blocker.SurfaceArea.Should().BeLessOrEqualTo(0.1f, $"rotation: {rotation.Value}");
                    blocker.SurfaceArea.Should().BeGreaterOrEqualTo(0.01f, $"rotation: {rotation.Value}");
                }
            }).Check(FsCheckUtils.Config);
        }

        [TestCase(0, 0, 0, 1)]
        [TestCase(45, 0, 0, 0.5f)]
        [TestCase(90, 0, 0, 0)]
        [TestCase(135, 0, 0, 0.5f)]
        [TestCase(180, 0, 0, 1)]
        [TestCase(225, 0, 0, 0.5f)]
        [TestCase(270, 0, 0, 0)]
        [TestCase(315, 0, 0, 0.5f)]
        [TestCase(0, 45, 0, 1)]
        [TestCase(0, 90, 0, 1)]
        [TestCase(0, 135, 0, 1)]
        [TestCase(0, 180, 0, 1)]
        [TestCase(0, 225, 0, 1)]
        [TestCase(0, 270, 0, 1)]
        [TestCase(0, 315, 0, 1)]
        [TestCase(0, 0, 45, 0.5f)]
        [TestCase(0, 0, 90, 0)]
        [TestCase(0, 0, 135, 0.5f)]
        [TestCase(0, 0, 180, 1)]
        [TestCase(0, 0, 225, 0.5f)]
        [TestCase(0, 0, 270, 0)]
        [TestCase(0, 0, 315, 0.5f)]
        public void SurfaceAreaGetsCalculatedCorrectlyForNode(float rx, float ry, float rz, float surfaceArea)
        {
            var data = new AbsorberData
            {
                lightBlocker = new LightBlocker(),
                lightAbsorber = new LightAbsorber(),
                translation = new Translation { Value = new float3(0, 1000, 0) },
                Rotation = new Rotation { Value = quaternion.Euler(math.radians(new float3(rx, ry, rz))) },
                Node = new Node { Size = new float3(1, 0, 1) },
            };
            RunSystems(new[] { data }); 
            
            foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
            {
                var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                blocker.SurfaceArea.Should().BeApproximately(surfaceArea, 0.0001f);
            }
        }

        [TestCase(0, 0, 0, 0.1f)]
        [TestCase(45, 0, 0, 0.055f)]
        [TestCase(90, 0, 0, 0.01f)]
        [TestCase(135, 0, 0, 0.055f)]
        [TestCase(180, 0, 0, 0.1f)]
        [TestCase(225, 0, 0, 0.055f)]
        [TestCase(270, 0, 0, 0.01f)]
        [TestCase(315, 0, 0, 0.055f)]
        [TestCase(0, 45, 0, 0.1f)]
        [TestCase(0, 90, 0, 0.1f)]
        [TestCase(0, 135, 0, 0.1f)]
        [TestCase(0, 180, 0, 0.1f)]
        [TestCase(0, 225, 0, 0.1f)]
        [TestCase(0, 270, 0, 0.1f)]
        [TestCase(0, 315, 0, 0.1f)]
        [TestCase(0, 0, 45, 0.1f)]
        [TestCase(0, 0, 90, 0.1f)]
        [TestCase(0, 0, 135, 0.1f)]
        [TestCase(0, 0, 180, 0.1f)]
        [TestCase(0, 0, 225, 0.1f)]
        [TestCase(0, 0, 270, 0.1f)]
        [TestCase(0, 0, 315, 0.1f)]
        public void SurfaceAreaGetsCalculatedCorrectlyForInternode(float rx, float ry, float rz, float surfaceArea)
        {
            var data = new AbsorberData
            {
                lightBlocker = new LightBlocker(),
                lightAbsorber = new LightAbsorber(),
                translation = new Translation { Value = new float3(0, 1000, 0) },
                Rotation = new Rotation { Value = quaternion.Euler(math.radians(new float3(rx, ry, rz))) },
                Node = new Node { Size = new float3(0, 0, 0) },
                Internode = new Internode { Length = 1, Radius = 0.05f },
            };
            RunSystems(new[] { data });

            foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
            {
                var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                blocker.SurfaceArea.Should().BeApproximately(surfaceArea, 0.0001f);
            }
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
                m_Manager.AddComponentData(entity, absorber.Rotation);
                if (absorber.Node.HasValue) m_Manager.AddComponentData(entity, absorber.Node.Value); 
                if (absorber.Internode.HasValue) m_Manager.AddComponentData(entity, absorber.Internode.Value);
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
            public Rotation Rotation;
            public Internode? Internode;
            public Node? Node;
        }
    }
}
