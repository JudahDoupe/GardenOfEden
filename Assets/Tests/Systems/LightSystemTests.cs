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
            from sa in FsCheckUtils.GenFloat(0, LightSystem.LightPerCell)
            from id in CoordinateTests.GenXyw(Coordinate.TextureWidthInPixels)
            select new LightBlocker { SurfaceArea = sa, CellId = id };

        public static Gen<LightAbsorber> GenLightAbsorber() =>
            from l in FsCheckUtils.GenFloat(0, LightSystem.LightPerCell)
            select new LightAbsorber { AbsorbedLight = l};

        private static Gen<Translation> GenTranslation() =>
            from y in FsCheckUtils.GenFloat(500, 5000)
            select new Translation { Value = new float3(0, y, 0) };
        
        private static Gen<Rotation> GenRotation() =>
            from euler in FsCheckUtils.GenFloat3(new float3(0,0,0), new float3(math.PI, math.PI, math.PI))
            select new Rotation { Value = quaternion.Euler(euler) };

        private static Gen<EntityData> GenAbsorberData() =>
            from lb in GenLightBlocker()
            from la in GenLightAbsorber()
            from t in GenTranslation()
            from r in GenRotation()
            select new EntityData {
                LightBlocker = lb,
                LightAbsorber = la,
                Translation = t,
                Rotation = r
            };

        private static Gen<EntityData> GenFullRandomAbsorberData() =>
            from lb in GenLightBlocker()
            from la in GenLightAbsorber()
            from translation in FsCheckUtils.GenFloat3(new float3(-1000, -1000, -1000), new float3(1000, 1000, 1000))
            from r in GenRotation()
            from size in FsCheckUtils.GenFloat3(new float3(0,0,0), new float3(1,1,1))
            from length in FsCheckUtils.GenFloat(0, 5f)
            from radius in FsCheckUtils.GenFloat(0, 10f)
            select new EntityData
            {
                LightBlocker = lb,
                LightAbsorber = la,
                Translation = new Translation { Value = translation },
                Rotation = r,
                Node = new Node{Size = size, InternodeLength = length, InternodeRadius = radius }
            };

        private static Arbitrary<EntityData[]> ArbAbsorberDataArray(int numEntities = 10) =>
            (from array in Gen.ArrayOf(numEntities, GenAbsorberData())
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
                    item.LightAbsorber.AbsorbedLight = -1;
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
                data[0].Node = new Node { Size = new float3(1, 1, 1) };
                var maxArea = math.pow(math.sqrt(3), 2) / 2;

                RunSystems(data);

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var rotation = m_Manager.GetComponentData<Rotation>(entity);
                    blocker.SurfaceArea.Should().NotBe(float.NaN);
                    blocker.SurfaceArea.Should().NotBe(float.NegativeInfinity);
                    blocker.SurfaceArea.Should().NotBe(float.PositiveInfinity);
                    blocker.SurfaceArea.Should().BeLessOrEqualTo(maxArea, $"rotation: {rotation.Value}");
                    blocker.SurfaceArea.Should().BeGreaterOrEqualTo(0.999f, $"rotation: {rotation.Value}");
                }
            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void SurfaceAreaIsWithinRangeForInternode()
        {
            Prop.ForAll(ArbAbsorberDataArray(1), data =>
            {
                data[0].Node = new Node { Size = new float3(0,0,0), InternodeLength = 1, InternodeRadius = 0.05f };

                RunSystems(data);

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var rotation = m_Manager.GetComponentData<Rotation>(entity);
                    blocker.SurfaceArea.Should().NotBe(float.NaN);
                    blocker.SurfaceArea.Should().NotBe(float.NegativeInfinity);
                    blocker.SurfaceArea.Should().NotBe(float.PositiveInfinity);
                    blocker.SurfaceArea.Should().BeLessOrEqualTo(0.101f, $"rotation: {rotation.Value}");
                    blocker.SurfaceArea.Should().BeGreaterOrEqualTo(0.009f, $"rotation: {rotation.Value}");
                }
            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void SurfaceAreaShouldBeAPositiveNumber()
        {
            Prop.ForAll(GenFullRandomAbsorberData().ToArbitrary(), data =>
            {
                RunSystems(new []{data});

                foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
                {
                    var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                    var rotation = m_Manager.GetComponentData<Rotation>(entity);
                    blocker.SurfaceArea.Should().NotBe(float.NaN);
                    blocker.SurfaceArea.Should().NotBe(float.NegativeInfinity);
                    blocker.SurfaceArea.Should().NotBe(float.PositiveInfinity);
                    blocker.SurfaceArea.Should().BeGreaterOrEqualTo(0,data.ToErrorString());
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
            var data = new EntityData
            {
                LightBlocker = new LightBlocker(),
                LightAbsorber = new LightAbsorber(),
                Translation = new Translation { Value = new float3(0, 1000, 0) },
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
            var data = new EntityData
            {
                LightBlocker = new LightBlocker(),
                LightAbsorber = new LightAbsorber(),
                Translation = new Translation { Value = new float3(0, 1000, 0) },
                Rotation = new Rotation { Value = quaternion.Euler(math.radians(new float3(rx, ry, rz))) },
                Node = new Node { Size = new float3(0, 0, 0), InternodeLength = 1, InternodeRadius = 0.05f },
            };
            RunSystems(new[] { data });

            foreach (var entity in m_Manager.CreateEntityQuery(typeof(LightBlocker)).ToEntityArray(Allocator.Temp))
            {
                var blocker = m_Manager.GetComponentData<LightBlocker>(entity);
                blocker.SurfaceArea.Should().BeApproximately(surfaceArea, 0.0001f);
            }
        }

        private void RunSystems(EntityData[] data)
        {
            m_Manager.DestroyAndResetAllEntities();

            foreach(var absorber in data)
            {
                var entity = m_Manager.CreateEntity();
                m_Manager.AddComponentData(entity, absorber.LightAbsorber);
                m_Manager.AddComponentData(entity, absorber.LightBlocker);
                m_Manager.AddComponentData(entity, absorber.Translation);
                m_Manager.AddComponentData(entity, absorber.Rotation);
                if (absorber.Node.HasValue) m_Manager.AddComponentData(entity, absorber.Node.Value);
                m_Manager.AddComponent(entity, typeof(LocalToWorld));
                m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
            }

            World.GetOrCreateSystem<EndFrameTRSToLocalToWorldSystem>().Update();
            World.GetOrCreateSystem<LightSystem>().Update();
        }

        private class EntityData
        {
            public LightBlocker LightBlocker;
            public LightAbsorber LightAbsorber;
            public Translation Translation;
            public Rotation Rotation;
            public Node? Node;

            public string ToErrorString()
            {
                return $@"
Translation: {Translation.Value}
Rotation: {Rotation.Value}
Internode length: {Node?.InternodeLength}
Internode radius: {Node?.InternodeRadius}
Node: {Node?.Size}
";
            }
        }
    }
}
