using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LandService : MonoBehaviour
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
        var color = ComputeShaderUtils.GetCachedTexture(LandMap).GetPixelBilinear(uv.x, uv.y);
        return color.r;
    }

    public float SampleWaterDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(SoilWaterMap).GetPixelBilinear(uv.x, uv.y);
        return color.b;
    }

    public float SampleRootDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(LandMap).GetPixelBilinear(uv.x, uv.y);
        return color.g;
    }

    public float SampleTerrainHeight(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(LandMap).GetPixelBilinear(uv.x, uv.y);
        return color.a;
    }

    public Vector3 ClampAboveTerrain(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(LandMap).GetPixelBilinear(uv.x, uv.y);
        location.y = color.a;
        return location;
    }
    public Vector3 ClampWithinSoil(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(LandMap).GetPixelBilinear(uv.x, uv.y);
        location.y = Mathf.Clamp(location.y, color.a - color.r, color.a);
        return location;
    }

    public void SetRoots(List<RootData> roots)
    {
        var kernelId = SoilShader.FindKernel("UpdateSoil");
        if (!roots.Any())
        {
            roots.Add(new RootData());
        }

        _rootBuffer?.Release();
        _rootBuffer = new ComputeBuffer(roots.Count, sizeof(float) * 5 + sizeof(int));
        _rootBuffer.SetData(roots);
        SoilShader.SetBuffer(kernelId, "RootBuffer", _rootBuffer);
        SoilShader.SetInt("NumRoots", roots.Count);
    }

    /* Inner Mechanations */

    private ComputeBuffer _rootBuffer;

    void Start()
    {
        var kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.SetTexture(kernelId, "SoilWaterMap", SoilWaterMap);
        SoilShader.SetTexture(kernelId, "LandMap", LandMap);
        SoilShader.SetTexture(kernelId, "WaterMap", WaterMap);
        SetRoots(new List<RootData>());
    }

    void FixedUpdate()
    {
        int kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.SetFloat("RootPullSpeed", RootPullSpeed);
        SoilShader.SetFloat("WaterAbsorptionRate", WaterAbsorptionRate);
        SoilShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        ComputeShaderUtils.UpdateTexture(LandMap);
        ComputeShaderUtils.UpdateTexture(SoilWaterMap);
    }
}
