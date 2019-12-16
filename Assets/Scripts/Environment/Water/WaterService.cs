using UnityEngine;

public class WaterService : MonoBehaviour
{
    [Header("Variables")]
    public float Rain_MetersPerSecond = 0.1f;


    [Header("Render Textures")]

    public RenderTexture TerrainHeightMap;
    public RenderTexture WaterSourceHeightMap;
    public RenderTexture WaterMap;

    [Header("Compute Shader")]
    public ComputeShader WaterShader;

    /* Publicly Accessible Methods */

    public float SampleWaterDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(WaterMap).GetPixelBilinear(uv.x, uv.y);
        return color.b;
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
        ComputeShaderUtils.ResetTexture(WaterMap);

        var updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "TerrainHeightMap", TerrainHeightMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", WaterSourceHeightMap);

        var rainKernel = WaterShader.FindKernel("Rain");
        WaterShader.SetTexture(rainKernel, "WaterMap", WaterMap);
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
        if (Input.GetKeyDown(KeyCode.U))
        {
            ComputeShaderUtils.ResetTexture(WaterMap);
        }
    }

    private void UpdateWaterTable()
    {
        int updateKernel = WaterShader.FindKernel("Update");
        WaterShader.Dispatch(updateKernel, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        ComputeShaderUtils.InvalidateCache(WaterMap);
    }
}
