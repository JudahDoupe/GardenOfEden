using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(PlateTectonicsSimulation))]
public class PateTectonicsGenerator : MonoBehaviour
{
    public ComputeShader LandGenerationShader;
    [Range(1, 30)]
    public int NumPlates = 16;
    [Range(0, 100)]
    public float FaultLineNoise = 0.25f;

    public void Regenerate() => Generate(NumPlates);
    public void Generate(int numPlates)
    {
        var plateTectonics = FindObjectOfType<PlateTectonicsSimulation>();
        var water = FindObjectOfType<WaterSimulation>();

        foreach (var plate in plateTectonics.GetAllPlates())
        {
            plateTectonics.RemovePlate(plate.Id);
        }

        for (int p = 0; p < numPlates; p++)
        {
            var plate = plateTectonics.AddPlate(p + 1.0001f);
            plate.Rotation = Random.rotation;
        }
        plateTectonics.Data.NeedsRegeneration = false;

        RunTectonicKernel(plateTectonics, "ResetMaps");
        plateTectonics.UpdateHeightMap();
        water.Regenerate();
    }

    private void RunTectonicKernel(PlateTectonicsSimulation sim, string kernelName)
    {
        int kernel = LandGenerationShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(NumPlates, Marshal.SizeOf(typeof(PlateGpuData)));
        buffer.SetData(Singleton.PlateTectonics.GetAllPlates().Select(x => x.ToGpuData()).ToArray());
        LandGenerationShader.SetBuffer(kernel, "Plates", buffer);
        LandGenerationShader.SetTexture(kernel, "PlateThicknessMaps", sim.Data.PlateThicknessMaps.RenderTexture);
        LandGenerationShader.SetTexture(kernel, "ContinentalIdMap", sim.Data.ContinentalIdMap.RenderTexture);
        LandGenerationShader.SetInt("NumPlates", NumPlates);
        LandGenerationShader.SetFloat("MantleHeight", sim.MantleHeight);
        LandGenerationShader.SetFloat("FaultLineNoise", FaultLineNoise);
        LandGenerationShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}