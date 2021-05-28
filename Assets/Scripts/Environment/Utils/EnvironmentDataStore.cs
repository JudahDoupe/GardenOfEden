using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class EnvironmentDataStore : MonoBehaviour
{
    public static RenderTexture WaterSourceMap;
    public static RenderTexture WaterMap;
    public static RenderTexture LandMap;

    public bool Regenerate;

    public float IslandFrequency;
    public float IslandSize;
    public float Mountains;
    public float MountainHeight; 

    void Awake()
    {
        LandMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture().Initialize();
        WaterMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture().Initialize();
        WaterSourceMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture().Initialize();
    }

    //TODO: Planet Factory

    void Update()
    {
        if (Regenerate)
        {
            RegenerateTerrain();
        }
    }

    void RegenerateTerrain()
    {
        ComputeShader cs = (ComputeShader)Resources.Load("Shaders/TerrainGenerator");
        var kernelId = cs.FindKernel("Generate");
        cs.SetTexture(kernelId, "LandMap", LandMap);
        cs.SetTexture(kernelId, "WaterMap", WaterMap);
        cs.SetFloat("Seed", Random.value);
        cs.SetFloat("SeaLevel", LandService.SeaLevel);
        cs.SetFloat("PlateauHeight", 20);
        cs.SetFloat("Smoothness", 1f);
        cs.SetFloat("Min", 100);
        cs.SetFloat("Max", 300);
        cs.Dispatch(kernelId, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}