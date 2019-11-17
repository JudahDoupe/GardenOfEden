﻿using UnityEngine;

public class WaterService : MonoBehaviour
{
    [Header("Debug")]
    public bool ResetWaterOnStart;

    [Header("Render Textures")]
    public RenderTexture Input;
    public RenderTexture Output;

    public RenderTexture TerrainHeightMap;
    public RenderTexture WaterSourceHeightMap;
    public RenderTexture WaterMap;

    [Header("Compute Shader")]
    public ComputeShader WaterShader;
    public ComputeShader SubtractShader;

    public Texture2D AbsorbWater(Texture2D rootMap, float multiplier)
    {
        if (rootMap == null)
        {
            ComputeShaderUtils.ResetTexture(Input);
        }
        else
        {
            Graphics.Blit(rootMap, Input);
        }

        int kernelId = SubtractShader.FindKernel("CSMain");
        SubtractShader.SetTexture(kernelId, "Base", WaterMap);
        SubtractShader.SetTexture(kernelId, "Mask", Input);
        SubtractShader.SetTexture(kernelId, "Result", Output);
        SubtractShader.SetFloat("Multiplier", multiplier);
        SubtractShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        return WaterMap.ToTexture2D();
    }

    public UnitsOfWater SampleWater(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = WaterMap.ToTexture2D().GetPixelBilinear(uv.x, uv.y);
        return UnitsOfWater.FromPixel(color.b);
    }

    public void Rain(float meters) //TODO: wire this up
    {
        int kernelId = WaterShader.FindKernel("Rain");
        WaterShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }

    /* Inner Mechanations */

    void Start()
    {
        ComputeShaderUtils.ResetTexture(Input);
        ComputeShaderUtils.ResetTexture(Output);
        if (ResetWaterOnStart) ComputeShaderUtils.ResetTexture(WaterMap);

        var updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "TerrainHeightMap", TerrainHeightMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", WaterSourceHeightMap);
        WaterShader.SetTexture(updateKernel, "Result", Output);
        WaterShader.SetTexture(updateKernel, "Test", Input);

        var rainKernel = WaterShader.FindKernel("Rain");
        WaterShader.SetTexture(rainKernel, "WaterMap", WaterMap);

        var hfsKernel = WaterShader.FindKernel("SuppressHighFrequencies");
        WaterShader.SetTexture(hfsKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(hfsKernel, "WaterSourceMap", WaterSourceHeightMap);
        WaterShader.SetTexture(hfsKernel, "TerrainHeightMap", TerrainHeightMap);
    }

    void FixedUpdate()
    {
        UpdateWaterTable();
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            Rain(0.25f);
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.U))
        {
            ComputeShaderUtils.ResetTexture(WaterMap);
        }
    }

    private void UpdateWaterTable()
    {
        int updateKernel = WaterShader.FindKernel("Update");
        WaterShader.Dispatch(updateKernel, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        Graphics.CopyTexture(Output, WaterMap);

        var hfsKernel = WaterShader.FindKernel("SuppressHighFrequencies");
        WaterShader.Dispatch(hfsKernel, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }
}
