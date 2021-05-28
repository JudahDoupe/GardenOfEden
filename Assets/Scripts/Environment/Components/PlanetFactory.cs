using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFactory : MonoBehaviour
{
    public bool Regenerate;

    public float IslandSize;
    public float MountainHeight;

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
        cs.SetTexture(kernelId, "LandMap", EnvironmentDataStore.LandMap);
        cs.SetTexture(kernelId, "WaterMap", EnvironmentDataStore.WaterMap);
        cs.SetFloat("Seed", Random.value);
        cs.SetFloat("Min", MountainHeight);
        cs.SetFloat("Max", MountainHeight * IslandSize);
        cs.Dispatch(kernelId, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
