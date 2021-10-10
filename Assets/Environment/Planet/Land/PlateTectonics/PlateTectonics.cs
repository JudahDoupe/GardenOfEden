using Assets.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlateTectonics : MonoBehaviour
{
    [Header("Generation")]
    [Range(1, 30)]
    public int NumPlates = 2;
    public float OceanFloorHeight = 900;
    public float InitialPlateThickness = 10;
    [Range(0,100)]
    public float FaultLineNoise = 0.25f;

    [Header("Simulation")]
    [Range(1, 10)]
    public float MaxPlateSpeed = 5;
    [Range(0, 1)]
    public float PlateInertia = 0.3f;
    [Range(0, 0.1f)]
    public float SubductionRate = 0.001f;
    [Range(0, 0.1f)]
    public float InflationRate = 0.001f;
    public float MinPlateThickness = 5;

    public ComputeShader TectonicsShader;
    public List<Plate> Plates = new List<Plate>();

    [Header("Visualization")]
    public Material OutlineReplacementMaterial;
    public Material FaultLineMaterial;
    [Range(0, 10)]
    public int ShowIndividualPlate = 0;
    public void ShowFaultLines(bool show) 
    {
        StartCoroutine(AnimationUtils.AnimateFloat(1, show ? 0 : 0.3f, show ? 0.3f : 0, x => FaultLineMaterial.SetFloat("Transparency", x))); 
    }

    private void Start()
    {
        ShowFaultLines(false);
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);
    }
    private void Update()
    {
        if (Plates.Count != NumPlates)
        {
            Regenerate(NumPlates);
        }
    }

    public void Regenerate() => Regenerate(Plates.Count);
    public void Regenerate(int numPlates)
    {
        Plates.Clear();
        EnvironmentDataStore.PlateThicknessMaps.ResetTexture(numPlates * 6);
        OutlineReplacementMaterial.SetTexture("ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        OutlineReplacementMaterial.SetTexture("HeightMap", EnvironmentDataStore.LandHeightMap);

        for (int p = 1; p <= numPlates; p++)
        {
            var plate = new Plate
            {
                Id = p,
                Rotation = Random.rotation,
                Velocity = Vector3.zero,
            };
            Plates.Add(plate);
        }

        RunTectonicKernel("ResetPlateThicknessMaps");
        RunTectonicKernel("ResetContinentalIdMap");
        UpdateHeightMap();
        Singleton.Water.Regenerate();
    }

    public void ProcessDay()
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
            plate.Rotation = Quaternion.LookRotation(plate.Center + plate.Velocity, plate.Rotation * Vector3.up);
            plate.Velocity = Vector3.Lerp(plate.Velocity, Vector3.zero, 1 - PlateInertia);
        }
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
        EnvironmentDataStore.ContinentalIdMap.UpdateTextureCache();
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        EnvironmentDataStore.LandHeightMap.UpdateTextureCache();
    }
    private void RunTectonicKernel(string kernelName)
    {
        int kernel = TectonicsShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(NumPlates, Marshal.SizeOf(typeof(Plate.Data)));
        buffer.SetData(Plates.Select(x => x.ToData()).ToArray());
        TectonicsShader.SetBuffer(kernel, "Plates", buffer);
        TectonicsShader.SetTexture(kernel, "LandHeightMap", EnvironmentDataStore.LandHeightMap);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", EnvironmentDataStore.PlateThicknessMaps);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        TectonicsShader.SetInt("NumPlates", NumPlates);
        TectonicsShader.SetFloat("InitialThickness", InitialPlateThickness);
        TectonicsShader.SetFloat("OceanFloorHeight", OceanFloorHeight);
        TectonicsShader.SetFloat("SubductionRate", SubductionRate);
        TectonicsShader.SetFloat("MinThickness", MinPlateThickness);
        TectonicsShader.SetFloat("InflationRate", InflationRate);
        TectonicsShader.SetFloat("FaultLineNoise", FaultLineNoise);
        TectonicsShader.SetInt("RenderPlate", ShowIndividualPlate);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public class Plate
    {
        public int Id;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 Center => Rotation * Vector3.forward * (Singleton.Water.SeaLevel + 100);
        public Data ToData() => new Data { Id = Id, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]), Velocity = Velocity.ToFloat3() };

        public struct Data
        {
            public int Id;
            public float4 Rotation;
            public float3 Velocity;
        }
    }
}
