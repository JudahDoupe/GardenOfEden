using UnityEngine;

public class WaterService : MonoBehaviour
{
    [Header("Variables")]
    public float Rain_MetersPerSecond = 0.1f;


    [Header("Render Textures")]

    public RenderTexture TerrainHeightMap;
    public RenderTexture WaterSourceHeightMap;
    public RenderTexture WaterMap;
    public RenderTexture WaterOutput;
    public RenderTexture RootInput;

    [Header("Compute Shader")]
    public ComputeShader WaterShader;
    public ComputeShader SubtractShader;

    /* Pubically Accessable Methods */

    public Texture2D AbsorbWater(Texture2D rootMap, float multiplier)
    {
        if (rootMap == null)
        {
            ComputeShaderUtils.ResetTexture(RootInput);
        }
        else
        {
            Graphics.Blit(rootMap, RootInput);
        }

        int kernelId = SubtractShader.FindKernel("CSMain");
        SubtractShader.SetTexture(kernelId, "Base", WaterMap);
        SubtractShader.SetTexture(kernelId, "Mask", RootInput);
        SubtractShader.SetTexture(kernelId, "Result", WaterOutput);
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

    public void Rain(float meters)
    {
        int kernelId = WaterShader.FindKernel("Rain");
        WaterShader.SetFloat("RainDepthInMeters", meters);
        WaterShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }

    /* Inner Mechanations */

    void Start()
    {
        ComputeShaderUtils.ResetTexture(RootInput);
        ComputeShaderUtils.ResetTexture(WaterOutput);
        ComputeShaderUtils.ResetTexture(WaterMap);

        var updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "TerrainHeightMap", TerrainHeightMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", WaterSourceHeightMap);
        WaterShader.SetTexture(updateKernel, "Result", WaterOutput);

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

        if (Input.GetKey(KeyCode.R))
        {
            Rain(Rain_MetersPerSecond / 60.0f);
        }
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.U))
        {
            ComputeShaderUtils.ResetTexture(WaterMap);
        }
    }

    private void UpdateWaterTable()
    {
        int updateKernel = WaterShader.FindKernel("Update");
        WaterShader.Dispatch(updateKernel, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        Graphics.CopyTexture(WaterOutput, WaterMap);

        var hfsKernel = WaterShader.FindKernel("SuppressHighFrequencies");
        WaterShader.Dispatch(hfsKernel, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }
}
