using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(PateTectonicsGenerator))]
[RequireComponent(typeof(PlateTectonicsAudio))]
[RequireComponent(typeof(PlateTectonicsVisualization))]
[RequireComponent(typeof(PlateBaker))]
[RequireComponent(typeof(MergePlateTool))]
[RequireComponent(typeof(BreakPlateTool))]
[RequireComponent(typeof(MovePlateTool))]
public class PlateTectonicsSimulation : MonoBehaviour, ISimulation
{
    public ComputeShader TectonicsShader;
    public float MantleHeight = 900;
    public float OceanicCrustThickness = 25;
    [Range(0, 0.1f)]
    public float SubductionRate = 0.001f;
    [Range(0, 0.1f)]
    public float InflationRate = 0.001f;
    [Range(0.00001f, 10f)]
    public float Gravity = 1f;
    [Range(1, 2)]
    public float PlateCohesion = 1.5f;
    [Range(0, 1)]
    public float PlateInertia = 0.3f;
    public float PlateSpeed = 500;

    private PlateTectonicsData _data;
    public bool IsInitialized => _data != null;
    public bool IsActive { get; private set; }

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
        GetComponent<PlateBaker>().Initialize(_data);
        GetComponent<PlateTectonicsVisualization>().Initialize(_data);
        GetComponent<PlateTectonicsAudio>().Initialize(_data);
        FindObjectOfType<PlateTectonicsToolbar>().Initialize(data, this, GetComponent<PlateTectonicsVisualization>());
    }
    public void Enable()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning($"{nameof(PlateTectonicsSimulation)} cannot be activated before it has been initialized.");
            return;
        }
        GetComponent<PlateTectonicsAudio>().Enable();
        GetComponent<PlateTectonicsVisualization>().Enable();
        IsActive = true;
    }
    public void Disable()
    {
        SimulationDataStore.UpdatePlateTectonics(_data);
        GetComponent<PlateTectonicsAudio>().Disable();
        GetComponent<PlateTectonicsVisualization>().Disable();
        IsActive = false;
    }

    public void UpdateSystem()
    {
        _data.MantleHeight = MantleHeight;
        UpdateVelocity();
        UpdateContinentalIdMap();
        UpdatePlateThicknessMaps();
        UpdateHeightMap();
    }
    public void UpdateVelocity()
    {
        float frameRateMultiplier = (60 * Math.Min(Time.deltaTime, 1 / 60f));
        foreach (var plate in _data.Plates)
        {
            var velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (1 - PlateInertia));
            plate.Velocity = Quaternion.Slerp(Quaternion.identity, velocity, PlateSpeed * frameRateMultiplier);
            plate.Rotation *= plate.Velocity;
        }
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
        _data.ContinentalIdMap.RefreshCache();
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        RunTectonicKernel("SmoothPlates");
        _data.LandHeightMap.RefreshCache();
    }
  
    private void RunTectonicKernel(string kernelName)
    {
        float frameRateMultiplier = (60 * Math.Min(Time.deltaTime, 1/60f));
        int kernel = TectonicsShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(_data.Plates.Count, Marshal.SizeOf(typeof(PlateGpuData)));
        buffer.SetData(_data.Plates.Select(x => x.ToGpuData()).ToArray());
        TectonicsShader.SetBuffer(kernel, "Plates", buffer);
        TectonicsShader.SetTexture(kernel, "LandHeightMap", _data.LandHeightMap.RenderTexture);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "TmpPlateThicknessMaps", _data.TmpPlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        TectonicsShader.SetInt("NumPlates", _data.Plates.Count);
        TectonicsShader.SetFloat("OceanicCrustThickness", OceanicCrustThickness);
        TectonicsShader.SetFloat("MantleHeight", MantleHeight);
        TectonicsShader.SetFloat("SubductionRate", SubductionRate * frameRateMultiplier);
        TectonicsShader.SetFloat("InflationRate", InflationRate * frameRateMultiplier);
        TectonicsShader.SetFloat("Gravity", Gravity * frameRateMultiplier);
        TectonicsShader.SetFloat("PlateCohesion", PlateCohesion * frameRateMultiplier);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private void Update()
    {
        if (IsActive)
        {
            UpdateSystem();
        }
    }
}

