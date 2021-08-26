using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlateTectonics : MonoBehaviour
{
    [Range(1, 30)]
    public int NumPlates = 2;
    [Range(500, 1000)]
    public float OceanFloorHeight = 900;
    [Range(1, 10)]
    public float MaxPlateSpeed = 5;
    [Range(0, 1)]
    public float SubductionRate = 0.01f;

    public ComputeShader TectonicsShader;
    public List<Plate> Plates = new List<Plate>();

    private void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);
    }
    private void Update()
    {
        if (Plates.Count != NumPlates)
        {
            Regenerate(NumPlates);
        }
    }

    public void Regenerate(int numPlates)
    {
        Plates.Clear();
        EnvironmentDataStore.PlateThicknessMaps.ResetTexture(numPlates);
        LandService.Renderer.material.SetTexture("ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        LandService.Renderer.material.SetInt("NumTectonicPlates", numPlates);

        for (int p = 0; p < numPlates; p++)
        {
            var plate = new Plate
            {
                Id = p,
                Rotation = Random.rotation,
            };
            Plates.Add(plate);
        }

        RunTectonicKernel("GeneratePlate");
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
            plate.Rotation = Quaternion.LookRotation(plate.Center + plate.Velocity, Vector3.up);
        }
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
        EnvironmentDataStore.PlateThicknessMaps.UpdateTextureCache();
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        EnvironmentDataStore.LandHeightMap.UpdateTextureCache();
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
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
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps_Read", EnvironmentDataStore.PlateThicknessMaps);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        TectonicsShader.SetInt("NumPlates", NumPlates);
        TectonicsShader.SetFloat("OceanFloorHeight", OceanFloorHeight);
        TectonicsShader.SetFloat("SubductionRate", SubductionRate);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public class Plate
    {
        public int Id;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 Center => Rotation * Vector3.forward * (Singleton.Water.SeaLevel + 100);
        public Data ToData() => new Data { Id = Id, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]) };

        public struct Data
        {
            public int Id;
            public float4 Rotation;
        }
    }
}
