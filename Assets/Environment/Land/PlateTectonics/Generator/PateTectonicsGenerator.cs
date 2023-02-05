using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlateTectonicsSimulation))]
public class PateTectonicsGenerator : Singleton<PateTectonicsGenerator>
{
    public ComputeShader LandGenerationShader;
    [Range(1, 30)]
    public int NumPlates = 16;
    [Range(750, 1000)]
    public float MantleHeight = 900;
    [Range(0, 100)]
    public float FaultLineNoise = 0.25f;

    public static PlateTectonicsData Generate(string planetName)
    {
        var data = new PlateTectonicsData(planetName)
        {
            MantleHeight = Instance.MantleHeight
        };

        for (int p = data.Plates.Count; p < Instance.NumPlates; p++)
        {
            var plate = data.AddPlate(p + 1.0001f);
            plate.Rotation = Random.rotation;
        }

        Instance.RunTectonicKernel(data, "ResetMaps");
        data.ContinentalIdMap.RefreshCache();
        data.LandHeightMap.RefreshCache();

        foreach (var plate in data.Plates)
        {
            plate.Rotation = Quaternion.identity;
        }

        return data;
    }

    private void RunTectonicKernel(PlateTectonicsData data, string kernelName)
    {
        int kernel = LandGenerationShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(NumPlates, Marshal.SizeOf(typeof(PlateGpuData)));
        buffer.SetData(data.Plates.Select(x => x.ToGpuData()).ToArray());
        LandGenerationShader.SetBuffer(kernel, "Plates", buffer);
        LandGenerationShader.SetTexture(kernel, "PlateThicknessMaps", data.PlateThicknessMaps.RenderTexture);
        LandGenerationShader.SetTexture(kernel, "ContinentalIdMap", data.ContinentalIdMap.RenderTexture);
        LandGenerationShader.SetTexture(kernel, "LandHeightMap", data.LandHeightMap.RenderTexture);
        LandGenerationShader.SetInt("NumPlates", NumPlates);
        LandGenerationShader.SetFloat("MantleHeight", data.MantleHeight);
        LandGenerationShader.SetFloat("FaultLineNoise", FaultLineNoise);
        LandGenerationShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}