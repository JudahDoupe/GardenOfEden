using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LiteDB;
using UnityEngine;

[RequireComponent(typeof(PateTectonicsGenerator))]
[RequireComponent(typeof(PlateTectonicsAudio))]
[RequireComponent(typeof(PlateTectonicsVisualization))]
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

    private bool _isInitialized => Data != null;
    private bool _isActive = false;
    public bool IsActive { 
        get => _isActive; 
        set
        {
            _isActive = value && _isInitialized;
            if(value && !_isInitialized)
                Debug.LogWarning($"{nameof(PlateTectonicsSimulation)} cannot be activated before it has been initialized.");

            FindObjectOfType<PlateTectonicsAudio>().IsActive = _isActive;
            FindObjectOfType<PlateTectonicsVisualization>().IsActive = _isActive;
        }
    }

    public PlateTectonicsData Data { get; private set; }
    public void Initialize(PlateTectonicsData data)
    {
        Data = data;
        if (data.NeedsRegeneration)
        {
            FindObjectOfType<PateTectonicsGenerator>().Regenerate();
            data.NeedsRegeneration = false;
        }
        FindObjectOfType<PlateTectonicsVisualization>().Initialize();
    }

    public List<PlateData> GetAllPlates() => Data.Plates.ToList();
    public PlateData GetPlate(float id) => Data.Plates.First(x => Math.Abs(x.Id - id) < float.Epsilon);
    public PlateData AddPlate() => AddPlate(Data.Plates.Max(x => x.Id) + 1f);
    public PlateData AddPlate(float id)
    {
        var plate = new PlateData(id, Data.Plates.Count);
        var currentLayerCount = Data.Plates.Count * 6;
        var newLayerCount = (Data.Plates.Count + 1) * 6;

        if (Data.Plates.Count > 0)
        {
            Graphics.CopyTexture(Data.PlateThicknessMaps.RenderTexture, Data.TmpPlateThicknessMaps.RenderTexture);
        }

        Data.PlateThicknessMaps.Layers = newLayerCount;
        for (var i = 0; i < currentLayerCount; i++)
        {
            Graphics.CopyTexture(Data.TmpPlateThicknessMaps.RenderTexture, i, Data.PlateThicknessMaps.RenderTexture, i);
        }
        Data.TmpPlateThicknessMaps.Layers = newLayerCount;

        Data.Plates.Add(plate);
        return plate;
    }
    public void RemovePlate(float id)
    {
        var plate = GetPlate(id);
        if (plate == null) return;
        
        Data.Plates.Remove(plate);
        var newLayerCount = Data.Plates.Count * 6;

        Graphics.CopyTexture(Data.PlateThicknessMaps.RenderTexture, Data.TmpPlateThicknessMaps.RenderTexture);
        Data.PlateThicknessMaps.Layers = newLayerCount;
        foreach (var p in Data.Plates)
        {
            var newIdx = Data.Plates.IndexOf(p);
            for (var i = 0; i < 6; i++)
            {
                Graphics.CopyTexture(Data.TmpPlateThicknessMaps.RenderTexture, (p.Idx * 6) + i, Data.PlateThicknessMaps.RenderTexture, (newIdx * 6) + i);
            }
            p.Idx = newIdx;
        }
        Data.TmpPlateThicknessMaps.Layers = newLayerCount;
    }

    public void UpdateSystem()
    {
        UpdateVelocity();
        UpdateContinentalIdMap();
        UpdatePlateThicknessMaps();
        UpdateHeightMap();
    }
    public void UpdateVelocity()
    {
        foreach (var plate in Data.Plates)
        {
            var velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (1 - PlateInertia));
            plate.Velocity = Quaternion.Slerp(Quaternion.identity, velocity, PlateSpeed * Time.deltaTime);
            plate.Rotation *= plate.Velocity;
        }
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
        Data.ContinentalIdMap.RefreshCache();
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        RunTectonicKernel("SmoothPlates");
        Data.LandHeightMap.RefreshCache();
    }
  
    private void RunTectonicKernel(string kernelName)
    {
        int kernel = TectonicsShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(Data.Plates.Count, Marshal.SizeOf(typeof(PlateGpuData)));
        buffer.SetData(Data.Plates.Select(x => x.ToGpuData()).ToArray());
        TectonicsShader.SetBuffer(kernel, "Plates", buffer);
        TectonicsShader.SetTexture(kernel, "LandHeightMap", Data.LandHeightMap.RenderTexture);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", Data.PlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "TmpPlateThicknessMaps", Data.TmpPlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", Data.ContinentalIdMap.RenderTexture);
        TectonicsShader.SetInt("NumPlates", Data.Plates.Count);
        TectonicsShader.SetFloat("OceanicCrustThickness", OceanicCrustThickness);
        TectonicsShader.SetFloat("MantleHeight", MantleHeight);
        TectonicsShader.SetFloat("SubductionRate", SubductionRate);
        TectonicsShader.SetFloat("InflationRate", InflationRate);
        TectonicsShader.SetFloat("Gravity", Gravity);
        TectonicsShader.SetFloat("PlateCohesion", PlateCohesion);
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

