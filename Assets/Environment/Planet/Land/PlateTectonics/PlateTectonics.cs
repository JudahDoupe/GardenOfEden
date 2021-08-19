using System.Collections.Generic;
using UnityEngine;

public class PlateTectonics : MonoBehaviour
{
    [Range(1, 30)]
    public int NumPlates = 2;
    [Range(500, 1000)]
    public float OceanFloorHeight = 900;

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

        for (int p = 0; p < numPlates; p++)
        {
            var plate = new Plate
            {
                Id = p,
                Rotation = Quaternion.identity,
            };
            Plates.Add(plate);
        }

        RunTectonicKernel("GeneratePlate");
    }

    public void ProcessDay()
    {
        UpdateHeightMap();
        UpdateContinentalIdMap();
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
        TectonicsShader.SetTexture(kernel, "LandHeightMap", EnvironmentDataStore.LandHeightMap);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", EnvironmentDataStore.PlateThicknessMaps);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        TectonicsShader.SetInt("NumPlates", NumPlates);
        TectonicsShader.SetFloat("OceanFloorHeight", OceanFloorHeight);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public class Plate
    {
        public int Id;
        public Quaternion Rotation;
        public Vector3 Center => Rotation * Vector3.up * (Singleton.Water.SeaLevel + 100);
    }
}
