﻿using System;
using UnityEngine;

public class WaterService : MonoBehaviour
{
    [Header("Variables")]
    public float Rain_MetersPerSecond = 0.1f;

    [Header("Compute Shader")]
    public ComputeShader WaterShader;
    public Renderer WaterRenderer;

    /* Publicly Accessible Methods */

    public float SampleWaterDepth(Vector3 location)
    {
        /*
        var uv = EnvironmentDataStore.LocationToUv(location);
        var color = Singleton.EnvironmentalChunkService.GetChunk(location).WaterMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
        return color.b;
        */
        return 0;
    }

    public void Rain(float meters)
    {
        /*
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            int kernelId = WaterShader.FindKernel("Rain");
            WaterShader.SetFloat("RainDepthInMeters", meters);
            WaterShader.SetTexture(kernelId, "WaterMap", chunk.WaterMap);
            WaterShader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
        }
        */
    }

    /* Inner Mechanations */

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        WaterRenderer.material.SetTexture("_WaterMap", EnvironmentDataStore.WaterMap);
        WaterRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000, 2000, 2000));
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
        /*
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            int updateKernel = WaterShader.FindKernel("Update");
            WaterShader.SetTexture(updateKernel, "LandMap", chunk.LandMap);
            WaterShader.SetTexture(updateKernel, "WaterMap", chunk.WaterMap);
            WaterShader.SetTexture(updateKernel, "WaterSourceMap", chunk.WaterSourceMap);
            WaterShader.Dispatch(updateKernel, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
        }
        */
    }

    public void ProcessDay()
    {
        /*
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            chunk.WaterMap.UpdateTextureCache();
        }
        */
    }
}