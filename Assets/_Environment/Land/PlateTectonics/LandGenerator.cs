using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(PlateTectonicsSimulation))]
public class LandGenerator : MonoBehaviour
{
    public ComputeShader LandGenerationShader;
    [Range(1, 30)]
    public int NumPlates = 16;
    [Range(0, 100)]
    public float FaultLineNoise = 0.25f;

    private PlateTectonicsSimulation simulation;

    private void Start()
    {
        simulation = GetComponent<PlateTectonicsSimulation>();
    }

    public void Regenerate() => Regenerate(NumPlates);
    public void Regenerate(int numPlates)
    {
        simulation.Initialize(new PlateTectonicsSimulationData());

        for (int p = 1; p <= numPlates; p++)
        {
            var plate = simulation.AddPlate(p + 0.0001f);
            plate.Rotation = Random.rotation;
        }

        RunTectonicKernel("ResetMaps");
        simulation.UpdateHeightMap();
        Singleton.Water.Regenerate();
    }

    private void RunTectonicKernel(string kernelName)
    {
        int kernel = LandGenerationShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(NumPlates, Marshal.SizeOf(typeof(PlateData)));
        buffer.SetData(simulation.GetAllPlates().Select(x => x.Serialize()).ToArray());
        LandGenerationShader.SetBuffer(kernel, "Plates", buffer);
        LandGenerationShader.SetTexture(kernel, "PlateThicknessMaps", EnvironmentMapDataStore.PlateThicknessMaps.RenderTexture);
        LandGenerationShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentMapDataStore.ContinentalIdMap.RenderTexture);
        LandGenerationShader.SetInt("NumPlates", NumPlates);
        LandGenerationShader.SetFloat("MantleHeight", simulation.MantleHeight);
        LandGenerationShader.SetFloat("FaultLineNoise", FaultLineNoise);
        LandGenerationShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}