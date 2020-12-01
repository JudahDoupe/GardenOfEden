using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public interface ILandService
{
    float SampleTerrainHeight(Vector3 location);
    Texture2D GetLandMap();
    Vector3 ClampAboveTerrain(Vector3 location);
    Vector3 ClampToTerrain(Vector3 location);
    public void PullMountain(Vector3 location, float height);
}

public class LandService : MonoBehaviour, ILandService
{
    [Header("Render Textures")]
    public RenderTexture LandMap;
    public RenderTexture SoilWaterMap;
    public RenderTexture WaterMap;
    //Bring back terrain cameras

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
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.a;
    }

    public Texture2D GetLandMap()
    {
        return LandMap.CachedTexture();
    }

    public Vector3 ClampAboveTerrain(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        location.y = Mathf.Max(color.a, location.y);
        return location;
    }
    public Vector3 ClampToTerrain(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        location.y = color.a;
        return location;
    }

    public void PullMountain(Vector3 location, float height)
    {
        var kernelId = SmoothAdd.FindKernel("SmoothAdd");
        SmoothAdd.SetTexture(kernelId, "Map", LandMap);
        SmoothAdd.SetFloats("Channels", 1,1,1,1);
        SmoothAdd.SetFloat("Strength", height);
        SmoothAdd.SetFloats("TextureCenter", 200,0,200);
        SmoothAdd.SetFloats("AdditionCenter", location.x, location.y, location.z);
        SmoothAdd.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        LandMap.UpdateTextureCache();
    }

    /* Inner Mechanations */

    void Start()
    {
        var kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.SetTexture(kernelId, "SoilWaterMap", SoilWaterMap);
        SoilShader.SetTexture(kernelId, "LandMap", LandMap);
        SoilShader.SetTexture(kernelId, "WaterMap", WaterMap);
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);
    }

    void FixedUpdate()
    {
        int kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.SetFloat("RootPullSpeed", RootPullSpeed);
        SoilShader.SetFloat("WaterAbsorptionRate", WaterAbsorptionRate);
        SoilShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }

    public void ProcessDay()
    {
        LandMap.UpdateTextureCache();
        SoilWaterMap.UpdateTextureCache();
    }
}
