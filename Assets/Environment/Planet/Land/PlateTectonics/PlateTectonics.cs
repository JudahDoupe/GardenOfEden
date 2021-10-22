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
    public int NumPlates = 16;
    public float MantleHeight = 900;
    [Range(0,100)]
    public float FaultLineNoise = 0.25f;

    [Header("Simulation")]
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
        EnvironmentDataStore.TmpPlateThicknessMaps.ResetTexture(numPlates * 6);
        OutlineReplacementMaterial.SetTexture("ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        OutlineReplacementMaterial.SetTexture("HeightMap", EnvironmentDataStore.LandHeightMap);

        for (int p = 1; p <= numPlates; p++)
        {
            var plate = new Plate
            {
                Id = p,
                Rotation = Random.rotation,
                Velocity = Quaternion.identity,
                TargetVelocity = Quaternion.identity,
            };
            Plates.Add(plate);
        }

        RunTectonicKernel("ResetPlateThicknessMaps");
        RunTectonicKernel("ResetContinentalIdMap");
        AlignPlateThicknessMaps();
        UpdateHeightMap();
        Singleton.Water.Regenerate();
    }

    public void ProcessDay()
    {
        UpdateVelocity();
        UpdateContinentalIdMap();
        UpdatePlateThicknessMaps();
        UpdateHeightMap();

        if (Plates.Any(x => !x.IsAligned) && Plates.All(x => x.IsStopped))
        {
            AlignPlateThicknessMaps();
        }
    }
    public void UpdateVelocity()
    {
        foreach (var plate in Plates)
        {
            //this integration needs to be frame rate independednt as well
            var velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (1 - PlateInertia));
            plate.Velocity = Quaternion.Slerp(Quaternion.identity, velocity, PlateSpeed * Time.deltaTime);
            plate.Rotation *= plate.Velocity;
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
        RunTectonicKernel("SmoothPlates");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        EnvironmentDataStore.LandHeightMap.UpdateTextureCache();
    }
    public void AlignPlateThicknessMaps()
    {
        RunTectonicKernel("StartAligningPlateThicknessMaps");
        foreach(var plate in Plates)
        {
            plate.Rotation = Quaternion.identity;
            plate.Velocity = Quaternion.identity;
        }
        RunTectonicKernel("FinishAligningPlateThicknessMaps");
    }
    private void RunTectonicKernel(string kernelName)
    {
        int kernel = TectonicsShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(NumPlates, Marshal.SizeOf(typeof(Plate.Data)));
        buffer.SetData(Plates.Select(x => x.ToData()).ToArray());
        TectonicsShader.SetBuffer(kernel, "Plates", buffer);
        TectonicsShader.SetTexture(kernel, "LandHeightMap", EnvironmentDataStore.LandHeightMap);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", EnvironmentDataStore.PlateThicknessMaps);
        TectonicsShader.SetTexture(kernel, "TmpPlateThicknessMaps", EnvironmentDataStore.TmpPlateThicknessMaps);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        TectonicsShader.SetInt("NumPlates", NumPlates);
        TectonicsShader.SetFloat("OceanicCrustThickness", OceanicCrustThickness);
        TectonicsShader.SetFloat("MantleHeight", MantleHeight);
        TectonicsShader.SetFloat("SubductionRate", SubductionRate);
        TectonicsShader.SetFloat("InflationRate", InflationRate);
        TectonicsShader.SetFloat("Gravity", Gravity);
        TectonicsShader.SetFloat("PlateCohesion", PlateCohesion);
        TectonicsShader.SetFloat("FaultLineNoise", FaultLineNoise);
        TectonicsShader.SetInt("RenderPlate", ShowIndividualPlate);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public class Plate
    {
        public int Id;
        public Quaternion Rotation;
        public Quaternion Velocity;
        public Quaternion TargetVelocity;
        public Vector3 Center => Rotation * Vector3.forward * (Singleton.Water.SeaLevel + 100);
        public bool IsStopped => Quaternion.Angle(Velocity, Quaternion.identity) < 0.001f;
        public bool IsAligned => Quaternion.Angle(Rotation, Quaternion.identity) < 0.001f;
        public Data ToData() => new Data { Id = Id, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]) };

        public struct Data
        {
            public int Id;
            public float4 Rotation;
        }
    }
}
