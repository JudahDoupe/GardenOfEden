using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlateTectonicsSimulation : MonoBehaviour, ISimulation
{
    [Header("Generation")]
    [Range(1, 30)]
    public int NumPlates = 16;
    public float MantleHeight = 900;
    [Range(0,100)]
    public float FaultLineNoise = 0.25f;
    public void Regenerate(int numPlates)
    {
        Plates.Clear();
        FindObjectOfType<PlateTectonicsVisualization>().Initialize();

        for (int p = 1; p <= numPlates; p++)
        {
            var plate = AddPlate(p + 0.0001f);
            plate.Rotation = Random.rotation;
        }

        RunTectonicKernel("ResetMaps");
        UpdateHeightMap();
        Singleton.Water.Regenerate();
    }

    [Header("Simulation")]
    public ComputeShader TectonicsShader;
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


    private bool _isActive;
    public bool IsActive { 
        get => _isActive; 
        set
        {
            _isActive = value;
            FindObjectOfType<PlateTectonicsAudio>().IsActive = value;
            FindObjectOfType<PlateTectonicsVisualization>().IsActive = value;
        }
    }

    private List<Plate> Plates = new List<Plate>();
    private EnvironmentMap TmpPlateThicknessMaps;
    private EnvironmentMap LandHeightMap => EnvironmentMapDataStore.LandHeightMap;
    private EnvironmentMap ContinentalIdMap => EnvironmentMapDataStore.ContinentalIdMap;
    private EnvironmentMap PlateThicknessMaps => EnvironmentMapDataStore.PlateThicknessMaps;

    public List<Plate> GetAllPlates() => Plates;
    public Plate GetPlate(float id) => Plates.First(x => Math.Abs(x.Id - id) < float.Epsilon);
    public Plate AddPlate() => AddPlate(Plates.Max(x => x.Id) + 1f);
    public Plate AddPlate(float id)
    {
        var plate = new Plate
        {
            Id = id,
            Idx = Plates.Count,
            Rotation = Quaternion.identity,
            Velocity = Quaternion.identity,
            TargetVelocity = Quaternion.identity,
        };
        var currentLayerCount = Plates.Count * 6;
        var newLayerCount = (Plates.Count + 1) * 6;

        if (Plates.Count > 0)
        {
            Graphics.CopyTexture(PlateThicknessMaps.RenderTexture, TmpPlateThicknessMaps.RenderTexture);
        }

        PlateThicknessMaps.Layers = newLayerCount;
        for (var i = 0; i < currentLayerCount; i++)
        {
            Graphics.CopyTexture(TmpPlateThicknessMaps.RenderTexture, i, PlateThicknessMaps.RenderTexture, i);
        }
        TmpPlateThicknessMaps.Layers = newLayerCount;

        Plates.Add(plate);
        NumPlates = Plates.Count;
        return plate;
    }
    public void RemovePlate(float id)
    {
        var plate = GetPlate(id);
        if (plate == null) return;
        
        Plates.Remove(plate);
        var newLayerCount = Plates.Count * 6;

        Graphics.CopyTexture(PlateThicknessMaps.RenderTexture, TmpPlateThicknessMaps.RenderTexture);
        PlateThicknessMaps.Layers = newLayerCount;
        foreach (var p in Plates)
        {
            var newIdx = Plates.IndexOf(p);
            for (var i = 0; i < 6; i++)
            {
                Graphics.CopyTexture(TmpPlateThicknessMaps.RenderTexture, (p.Idx * 6) + i, PlateThicknessMaps.RenderTexture, (newIdx * 6) + i);
            }
            p.Idx = newIdx;
        }
        TmpPlateThicknessMaps.Layers = newLayerCount;
        NumPlates = Plates.Count;
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
        foreach (var plate in Plates)
        {
            var velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (1 - PlateInertia));
            plate.Velocity = Quaternion.Slerp(Quaternion.identity, velocity, PlateSpeed * Time.deltaTime);
            plate.Rotation *= plate.Velocity;
        }
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
        ContinentalIdMap.RefreshCache();
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        RunTectonicKernel("SmoothPlates");
        LandHeightMap.RefreshCache();
    }
  
    private void RunTectonicKernel(string kernelName)
    {
        int kernel = TectonicsShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(NumPlates, Marshal.SizeOf(typeof(Plate.GpuData)));
        buffer.SetData(Plates.Select(x => x.ToGpuData()).ToArray());
        TectonicsShader.SetBuffer(kernel, "Plates", buffer);
        TectonicsShader.SetTexture(kernel, "LandHeightMap", LandHeightMap.RenderTexture);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", PlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "TmpPlateThicknessMaps", TmpPlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", ContinentalIdMap.RenderTexture);
        TectonicsShader.SetInt("NumPlates", NumPlates);
        TectonicsShader.SetFloat("OceanicCrustThickness", OceanicCrustThickness);
        TectonicsShader.SetFloat("MantleHeight", MantleHeight);
        TectonicsShader.SetFloat("SubductionRate", SubductionRate);
        TectonicsShader.SetFloat("InflationRate", InflationRate);
        TectonicsShader.SetFloat("Gravity", Gravity);
        TectonicsShader.SetFloat("PlateCohesion", PlateCohesion);
        TectonicsShader.SetFloat("FaultLineNoise", FaultLineNoise);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private void Start()
    {
        IsActive = false;
        TmpPlateThicknessMaps = new EnvironmentMap(EnvironmentMapType.PlateThicknessMaps);
    }
    private void Update()
    {
        if (Plates.Count != NumPlates)
        {
            Regenerate(NumPlates);
        }
        if (IsActive)
        {
            UpdateSystem();
        }
    }
}
