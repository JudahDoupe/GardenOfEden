using System;
using System.Linq;
using System.Runtime.InteropServices;
using FsCheck;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Tests
{
    [Category("Gpu")]
    public class PlanetTests : GpuTestBase
    {
        public static Gen<float3> GenCroppedUvw()
        {
            return from u in FsCheckUtils.GenFloat(1 / 512.0f, 511 / 512.0f)
                   from v in FsCheckUtils.GenFloat(1 / 512.0f, 511 / 512.0f)
                   from w in Gen.Choose(0, 5)
                   select new float3(u, v, w);
        }

        [Test]
        public void TestTextureSampling()
        {
            var texture = new RenderTexture(512, 512, 0, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture(6).InitializeXyw();
            Prop.ForAll(GenCroppedUvw().ToArbitrary(), uvw =>
            {
                TestHeightSample(uvw, 0.001f, texture);

            }).Check(FsCheckUtils.Config);
        }

        [TestCase(0,0)]
        [TestCase(0.25f,0)]
        [TestCase(0.5f,0)]
        [TestCase(1,0)]
        [TestCase(0, 0.25f)]
        [TestCase(0.25f, 0.25f)]
        [TestCase(0.5f, 0.25f)]
        [TestCase(1, 0.5f)]
        [TestCase(0, 0.5f)]
        [TestCase(0.25f, 0.5f)]
        [TestCase(0.5f, 0.5f)]
        [TestCase(1, 0.5f)]
        [TestCase(0, 1)]
        [TestCase(0.25f, 1)]
        [TestCase(0.5f, 1)]
        [TestCase(1, 1)]
        [TestCase(0.0009765625f, 0.0009765625f)]
        [TestCase(0.00048828125f, 0.00048828125f)]
        public void TestTextureSamplingExpliciate(float u, float v)
        {
            var texture = new RenderTexture(512, 512, 0, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture(6).InitializeXyw();
            for(var i = 0; i < 6; i++)
            {
                var uvw = new float3(u, v, 3);
                TestHeightSample(uvw, 0.00000001f, texture);
            }
        }

        private void TestHeightSample(float3 uvw, float percision, RenderTexture texture)
        {
            var input = new SamplerData[1];
            var output = new SamplerData[1];
            var coord = new Coordinate(Vector3.zero, Planet.LocalToWorld) { TextureUvw = uvw };
            input[0] = new SamplerData { uvw = uvw };

            using var buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(SamplerData)));
            buffer.SetData(input);
            var shader = Resources.Load<ComputeShader>("Shaders/PlanetTests");
            var kernelId = shader.FindKernel("Test_sampleHeightMap");
            shader.SetBuffer(kernelId, "coords", buffer);
            shader.SetTexture(kernelId, "_HeightMap", texture);
            shader.Dispatch(kernelId, 1, 1, 1);

            buffer.GetData(output);

            var color = texture.Sample(coord);
            var sampleValue = new float4(color.r, color.g, color.b, color.a);
            output[0].value.Should().BeApproximately(sampleValue, percision);
        }

        private struct SamplerData
        {
            public float3 uvw;
            public float4 value;
        }
    }
}
