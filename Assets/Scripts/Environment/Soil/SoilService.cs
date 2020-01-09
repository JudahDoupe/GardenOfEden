﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoilService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture SoilMap;
    public RenderTexture SoilWaterMap;
    public RenderTexture WaterMap;
    //Bring back terrain cameras

    [Header("Compute Shaders")]
    public ComputeShader SoilShader;

    /* Publicly Accessible Methods */

    public float SampleSoilDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(SoilMap).GetPixelBilinear(uv.x, uv.y);
        return color.r;
    }

    public float SampleWaterDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(SoilWaterMap).GetPixelBilinear(uv.x, uv.y);
        return color.b;
    }

    public float SampleTerrainHeight(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(SoilMap).GetPixelBilinear(uv.x, uv.y);
        return color.a;
    }

    public void SetRoots(List<RootData> roots)
    {
        var kernelId = SoilShader.FindKernel("UpdateSoil");
        if (!roots.Any())
        {
            roots.Add(new RootData());
        }

        _rootBuffer?.Release();
        _rootBuffer = new ComputeBuffer(roots.Count, sizeof(float) * 4 + sizeof(int));
        _rootBuffer.SetData(roots);
        SoilShader.SetBuffer(kernelId, "RootBuffer", _rootBuffer);
        SoilShader.SetInt("NumRoots", roots.Count);
    }

    /* Inner Mechanations */

    private ComputeBuffer _rootBuffer;

    void Start()
    {
        ComputeShaderUtils.ResetTexture(SoilMap);

        var kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.SetTexture(kernelId, "SoilWaterMap", SoilWaterMap);
        SoilShader.SetTexture(kernelId, "SoilMap", SoilMap);
        SoilShader.SetTexture(kernelId, "WaterMap", WaterMap);
        SetRoots(new List<RootData>());
    }

    void FixedUpdate()
    {
        int kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        ComputeShaderUtils.InvalidateCache(SoilMap);
    }
}
