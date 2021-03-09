using System;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Tests
{
    [Category("Gpu")]
    public class CoordinateTests : GpuTestBase
    {
        public static Gen<int3> GenXyw(int textureWidth)
        {
            return from x in Gen.Choose(0, textureWidth - 1)
                   from y in Gen.Choose(0, textureWidth - 1)
                   from w in Gen.Choose(0, 5)
                   select new int3(x,y,w);
        }

        public static Gen<float3> GenUvw()
        {
            return from u in FsCheckUtils.Gen0To1()
                   from v in FsCheckUtils.Gen0To1()
                   from w in Gen.Choose(0, 5)
                   select new float3(u,v,w);
        }

        public static Gen<float3> GenXyz()
        {
            return from x in FsCheckUtils.GenFloat(-1000, 1000)
                   from y in FsCheckUtils.GenFloat(-1000, 1000)
                   from z in FsCheckUtils.GenFloat(-1000, 1000)
                   where x != 0 && y != 0 && z != 0
                   select new float3(x,y,z);
        }

        [Test]
        public void TestXyz()
        {
            Prop.ForAll(GenXyz().ToArbitrary(), xyz =>
            {
                var input = new CoordData [1];
                var output = new CoordData [1];
                var expected = new Coordinate(xyz);
                input[0] = new CoordData { altitude = expected.Altitude, xyz = xyz };

                using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CoordData)));
                buffer.SetData(input);
                var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
                var kernelId = shader.FindKernel("Test_xyz");
                shader.SetBuffer(kernelId, "coords", buffer);
                shader.Dispatch(kernelId, 1, 1, 1);

                buffer.GetData(output);

                output[0].uvw.Should().BeApproximately(expected.uvw, 0.00001f);
                output[0].xyw.Should().Be(expected.xyw);

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void TestXyw()
        {
            Prop.ForAll(GenXyw(512).ToArbitrary(), xyw =>
            {
                var input = new CoordData[1];
                var output = new CoordData[1];
                var expected = new Coordinate(0, 0, 0) {xyw = xyw};
                input[0] = new CoordData { altitude = expected.Altitude, xyw = xyw };

                using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CoordData)));
                buffer.SetData(input);
                var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
                var kernelId = shader.FindKernel("Test_xyw");
                shader.SetBuffer(kernelId, "coords", buffer);
                shader.Dispatch(kernelId, 1, 1, 1);

                buffer.GetData(output);

                output[0].uvw.Should().BeApproximately(expected.uvw, 0.00001f);
                output[0].xyz.Should().BeApproximately(expected.xyz, 0.00001f);

            }).Check(FsCheckUtils.Config);
        }

        [Test]
        public void TestUvw()
        {
            Prop.ForAll(GenUvw().ToArbitrary(), uvw =>
            {
                var input = new CoordData[1];
                var output = new CoordData[1];
                var expected = new Coordinate(0, 0, 0) { uvw = uvw };
                input[0] = new CoordData { altitude = expected.Altitude, uvw = uvw };

                using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CoordData)));
                buffer.SetData(input);
                var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
                var kernelId = shader.FindKernel("Test_uvw");
                shader.SetBuffer(kernelId, "coords", buffer);
                shader.Dispatch(kernelId, 1, 1, 1);

                buffer.GetData(output);

                output[0].xyw.Should().Be(expected.xyw);
                output[0].xyz.Should().BeApproximately(expected.xyz, 0.00001f);

            }).Check(FsCheckUtils.Config);
        }

        private struct CoordData
        {
            public float altitude;
            public float3 uvw;
            public float3 xyz;
            public int3 xyw;
        }
    }
}
