using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public interface ILandService
{
    float SampleTerrainHeight(Vector3 location);
    Vector3 ClampAboveTerrain(Vector3 location);
    Vector3 ClampToTerrain(Vector3 location);
    public void PullMountain(Vector3 location, float height);
}

public class LandService : MonoBehaviour, ILandService
{
    [Header("Compute Shaders")]
    public ComputeShader SoilShader;
    public ComputeShader SmoothAdd;
    [Range(0.1f,0.5f)]
    public float RootPullSpeed = 0.1f;
    [Range(0.01f, 0.5f)]
    public float WaterAbsorptionRate = 0.02f;

    /* Publicly Accessible Methods */

    public float SampleTerrainHeight(Vector3 location)
    {
        var uv = EnvironmentalChunkService.LocationToUv(location);
        var color = Singleton.EnvironmentalChunkService.GetChunk(location).LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.a;
    }

    public Vector3 ClampAboveTerrain(Vector3 location)
    {
        var uv = EnvironmentalChunkService.LocationToUv(location);
        var color = Singleton.EnvironmentalChunkService.GetChunk(location).LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        location.y = Mathf.Max(color.a, location.y);
        return location;
    }
    public Vector3 ClampToTerrain(Vector3 location)
    {
        var uv = EnvironmentalChunkService.LocationToUv(location);
        var color = Singleton.EnvironmentalChunkService.GetChunk(location).LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        location.y = color.a;
        return location;
    }

    public void PullMountain(Vector3 location, float height)
    {
        if (isPullingMountain) return;

        SmoothAdd.SetFloat("Radius", height);
        SmoothAdd.SetFloats("Channels", 0, 0, 0, 1);
        SmoothAdd.SetFloats("TextureCenter", 200, 0, 200);
        SmoothAdd.SetFloats("AdditionCenter", location.x, location.y, location.z);

        StartCoroutine(SmoothPullMountain(height, 2f));
    }
    private bool isPullingMountain = false;
    private IEnumerator SmoothPullMountain(float height, float seconds)
    {
        isPullingMountain = true;
        var kernelId = SmoothAdd.FindKernel("SmoothAdd");
        var realHeight = 0f;
        while (realHeight < height)
        {
            var maxSpeed = height / seconds * Time.deltaTime;
            var growth = Mathj.Tween(realHeight, height) * maxSpeed;
            realHeight += growth;
            foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
            {
                SmoothAdd.SetFloat("Strength", growth);
                SmoothAdd.SetTexture(kernelId, "Map", chunk.LandMap);
                SmoothAdd.Dispatch(kernelId, EnvironmentalChunkService.TextureSize / 8, EnvironmentalChunkService.TextureSize / 8, 1);
                chunk.LandMap.UpdateTextureCache();
            }
            yield return new WaitForEndOfFrame();
        }
        isPullingMountain = false;
    }

    /* Inner Mechanations */

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);
    }

    void FixedUpdate()
    {
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            int kernelId = SoilShader.FindKernel("UpdateSoil");
            SoilShader.SetTexture(kernelId, "SoilWaterMap", chunk.SoilWaterMap);
            SoilShader.SetTexture(kernelId, "LandMap", chunk.LandMap);
            SoilShader.SetTexture(kernelId, "WaterMap", chunk.WaterMap);
            SoilShader.SetFloat("RootPullSpeed", RootPullSpeed);
            SoilShader.SetFloat("WaterAbsorptionRate", WaterAbsorptionRate);
            SoilShader.Dispatch(kernelId, EnvironmentalChunkService.TextureSize / 8, EnvironmentalChunkService.TextureSize / 8, 1);
        }
    }

    public void ProcessDay()
    {
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            chunk.LandMap.UpdateTextureCache();
            chunk.SoilWaterMap.UpdateTextureCache();
        }
    }
}
