using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    [Category("Gpu")]
    public class PlateTectonicsTests : GpuTestBase
    {
        [Test]
        public void VelocityIntegrationTests([Values(1,2,3)] int caseNumer)
        {
            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Environment/Planet/Land/PlateTectonics/Tectonics.compute");
            var continentalHeightMap = new RenderTexture(4, 4, 0, RenderTextureFormat.RFloat, 0).ResetTexture().Initialize();
            var continentalVelocityMap = new RenderTexture(4, 4, 0, RenderTextureFormat.RGFloat, 0).ResetTexture().Initialize();

            var textures = CSVReader.ReadTextures(@$"Assets\Tests\Gpu\TestCases\tectonics_velocity_integration_{caseNumer}.csv");
            continentalHeightMap.Initialize(textures[0]);
            continentalVelocityMap.Initialize(textures[1]);

            int kernel = shader.FindKernel("IntegrateVelocities");
            shader.SetTexture(kernel, "ContinentalHeightMap", continentalHeightMap);
            shader.SetTexture(kernel, "ContinentalVelocityMap", continentalVelocityMap);
            shader.SetFloat("OceanFloorAltitude", 0);
            shader.Dispatch(kernel, 1, 1, 1);

            continentalHeightMap.UpdateTextureCache();
            var output = continentalHeightMap.CachedTextures()[0].GetPixels(0,0,4,4);
            var expected = textures[2].SelectMany(x => x).ToArray();

            output.Should().BeEquivalentTo(expected);
        }
    }
}
