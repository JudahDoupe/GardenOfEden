/*
 using System;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Tests
{
    [Category("Gpu")]
    public class CoordinateTests : GpuTestBase
    {
        private LocalToWorld _planet;

        [SetUp]
        public void Setup()
        {
            _planet = new LocalToWorld
            {
                Value = new float4x4(quaternion.identity, new float3(0, 0, 0))
            };
        }

        [Test]
        public void TestXyz()
        {
            var data = new[]
            {
                new Coordinate(new float3(0, 0, 0), _planet),
                new Coordinate(new float3(1000, 0, 0), _planet),
                new Coordinate(new float3(0, 1000, 0), _planet),
                new Coordinate(new float3(0, 0, 1000), _planet),
                new Coordinate(new float3(1000, 1000, 1000), _planet),
                new Coordinate(new float3(752, 84, 159), _planet),
                new Coordinate(new float3(-1000, -10, -1), _planet),
                new Coordinate(new float3(0.1238f, 0.00000001f, 156824), _planet),
            };
            var input = data.Select(x => new CoordData{altitude = x.Altitude, xyz = x.Global(_planet) }).ToArray();
            var output = new CoordData [data.Length];

            using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CoordData)));
            buffer.SetData(input);
            var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
            var kernelId = shader.FindKernel("Test_xyz");
            shader.SetBuffer(kernelId, "coords", buffer);
            shader.Dispatch(kernelId, 1, 1, 1);

            buffer.GetData(output);

            for (int i = 0; i < data.Length; i++)
            {
                output[i].uvw.Should().BeApproximately(data[i].TextureUvw, 0.001f);
                output[i].xyw.Should().Be(data[i].TextureXyw);
            }
        }

        [Test]
        public void TestXyw()
        {
            Prop.ForAll(GenXyw(512).ToArbitrary(), xyw =>
            {
                var input = new CoordData[1];
                var output = new CoordData[1];
                var expected = new Coordinate(Vector3.up * 1000, _planet) {TextureXyw = xyw};
                input[0] = new CoordData { altitude = expected.Altitude, xyw = xyw };

                using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CoordData)));
                buffer.SetData(input);
                var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
                var kernelId = shader.FindKernel("Test_xyw");
                shader.SetBuffer(kernelId, "coords", buffer);
                shader.Dispatch(kernelId, 1, 1, 1);

                buffer.GetData(output);

                output[0].uvw.Should().BeApproximately(expected.TextureUvw, 0.001f);
                output[0].xyz.Should().BeApproximately(expected.Global(_planet), 0.001f);

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void TestUvw()
        {
            Prop.ForAll(GenUvw().ToArbitrary(), uvw =>
            {
                var input = new CoordData[1];
                var output = new CoordData[1];
                var expected = new Coordinate(Vector3.up * 1000, _planet) { TextureUvw = uvw };
                input[0] = new CoordData { altitude = expected.Altitude, uvw = uvw };

                using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CoordData)));
                buffer.SetData(input);
                var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
                var kernelId = shader.FindKernel("Test_uvw");
                shader.SetBuffer(kernelId, "coords", buffer);
                shader.Dispatch(kernelId, 1, 1, 1);

                buffer.GetData(output);

                output[0].xyw.Should().Be(expected.TextureXyw);
                output[0].xyz.Should().BeApproximately(expected.Global(_planet), 0.001f);

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void BoundryTests([Values(0,1,200,510,511)] int x, [Values(0, 1, 200, 510, 511)] int y)
        {
            var xyw = new int3(x, y, 0);

            var input = new CoordData[1];
            var output = new CoordData[1];
            var expected = new Coordinate(Vector3.up * 1000, _planet) { TextureXyw = xyw };
            input[0] = new CoordData { altitude = expected.Altitude, xyw = xyw };

            using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CoordData)));
            buffer.SetData(input);
            var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
            var kernelId = shader.FindKernel("Test_Boundries");
            shader.SetBuffer(kernelId, "coords", buffer);
            shader.Dispatch(kernelId, 1, 1, 1);

            buffer.GetData(output);

            var tw = Coordinate.TextureWidthInPixels - 1;
            var isBoundry = (x <= 0 || tw <= x || y <= 0 || tw <= y);
            if (isBoundry)
            {
                output[0].xyw.z.Should().NotBe(0);
                output[0].xyw.x.Should().NotBe(0);
                output[0].xyw.x.Should().NotBe(511);
                output[0].xyw.y.Should().NotBe(0);
                output[0].xyw.y.Should().NotBe(511);
                output[0].isBoundry.Should().Be(1);
            }
            else
            {
                output[0].xyw.z.Should().Be(0);
                output[0].xyw.x.Should().Be(x);
                output[0].xyw.y.Should().Be(y);
                output[0].isBoundry.Should().Be(0);
            }

            var maxDistance = (2 * math.PI * Coordinate.PlanetRadius) / (Coordinate.TextureWidthInPixels * 4);
            output[0].xyz.Should().BeApproximately(expected.Global(_planet), maxDistance);
        }

        private struct CoordData
        {
            public float altitude;
            public float3 uvw;
            public float3 xyz;
            public int3 xyw;
            public int isBoundry;
        }
    }
}

 */