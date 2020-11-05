using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ILandService : IDailyProcess
{
    float SampleSoilDepth(Vector3 location);
    float SampleWaterDepth(Vector3 location);
    float SampleRootDepth(Vector3 location);
    float SampleTerrainHeight(Vector3 location);
    Vector3 ClampAboveTerrain(Vector3 location);
    Vector3 ClampToTerrain(Vector3 location);
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
    [Range(0.1f,0.5f)]
    public float RootPullSpeed = 0.1f;
    [Range(0.01f, 0.5f)]
    public float WaterAbsorptionRate = 0.02f;

    /* Publicly Accessible Methods */

    public float SampleSoilDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.r;
    }

    public float SampleWaterDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = SoilWaterMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.b;
    }

    public float SampleRootDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.g;
    }

    public float SampleTerrainHeight(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = LandMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.a;
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

    /* Inner Mechanations */

    void Start()
    {
        var kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.SetTexture(kernelId, "SoilWaterMap", SoilWaterMap);
        SoilShader.SetTexture(kernelId, "LandMap", LandMap);
        SoilShader.SetTexture(kernelId, "WaterMap", WaterMap);
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

    public bool HasDayBeenProccessed()
    {
        return true;
    }
}
