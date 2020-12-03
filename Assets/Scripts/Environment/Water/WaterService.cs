﻿using System;
using UnityEngine;

public class WaterService : MonoBehaviour
{
    [Header("Variables")]
    public float Rain_MetersPerSecond = 0.1f;


    [Header("Render Textures")]

    public RenderTexture LandMap;
    public RenderTexture WaterSourceMap;
    public RenderTexture WaterMap;

    [Header("Compute Shader")]
    public ComputeShader WaterShader;

    /* Publicly Accessible Methods */

    public float SampleWaterDepth(Vector3 location)
    {
        var uv = EnvironmentalChunkService.LocationToUv(location);
        var color = WaterMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.b;
    }

    public void Rain(float meters)
    {
        int kernelId = WaterShader.FindKernel("Rain");
        WaterShader.SetFloat("RainDepthInMeters", meters);
        WaterShader.Dispatch(kernelId, EnvironmentalChunkService.TextureSize / 8, EnvironmentalChunkService.TextureSize / 8, 1);
    }

    /* Inner Mechanations */

    void Start()
    {
        var updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "LandMap", LandMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", WaterSourceMap);

        var rainKernel = WaterShader.FindKernel("Rain");
        WaterShader.SetTexture(rainKernel, "WaterMap", WaterMap);

        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);
    }

    void FixedUpdate()
    {
        UpdateWaterTable();

        if (Input.GetKey(KeyCode.R))
        {
            Rain(Rain_MetersPerSecond / 60.0f);
        }
    }

    private void UpdateWaterTable()
    {
        int updateKernel = WaterShader.FindKernel("Update");
        WaterShader.Dispatch(updateKernel, EnvironmentalChunkService.TextureSize / 8, EnvironmentalChunkService.TextureSize / 8, 1);
    }

    public void ProcessDay()
    {
        WaterMap.UpdateTextureCache();
    }
}
