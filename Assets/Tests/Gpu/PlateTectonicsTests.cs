using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    [Category("Gpu")]
    public class PlateTectonicsTests : GpuTestBase
    {
        [Test]
        public void VelocityIntegrationTests()
        {
            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Environment/Planet/Land/PlateTectonics/Tectonics.compute");
            var continentalIdMap = new RenderTexture(4, 4, 0, RenderTextureFormat.RFloat, 0).ResetTexture().Initialize();
            var continentalHeightMap = new RenderTexture(4, 4, 0, RenderTextureFormat.RFloat, 0).ResetTexture().Initialize();
            var continentalVelocityMap = new RenderTexture(4, 4, 0, RenderTextureFormat.RGFloat, 0).ResetTexture().Initialize();

            string searchPattern = "tectonics_velocity_*.csv";
            DirectoryInfo di = new DirectoryInfo(@"\TestCases");
            FileInfo[] files = di.GetFiles(searchPattern);

            for (var i = 0; i < files.Length; i++)
            {
                int kernel = shader.FindKernel("IntegrateVelocities");
                shader.SetTexture(kernel, "ContinentalIdMap", continentalIdMap);
                shader.SetTexture(kernel, "ContinentalHeightMap", continentalHeightMap);
                shader.SetTexture(kernel, "ContinentalVelocityMap", continentalVelocityMap);
                shader.SetFloat("OceanFloorAltitude", 0);
                shader.Dispatch(kernel, 1, 1, 1);
            }
        }
    }
}
