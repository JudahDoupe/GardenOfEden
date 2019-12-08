using UnityEngine;

public class SoilService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture BedrockHeightMap;
    public RenderTexture SoilHeightMap;
    public RenderTexture SoilMap;
    public RenderTexture WaterMap;
    public RenderTexture SoilOutput;

    [Header("Compute Shaders")]
    public ComputeShader SoilShader;

    /* Pubically Accessable Methods */

    public float SampleSoilDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(SoilMap).GetPixelBilinear(uv.x, uv.y);
        return color.r;
    }

    public float SampleWaterDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(SoilMap).GetPixelBilinear(uv.x, uv.y);
        return color.b;
    }

    /* Inner Mechanations */

    void Start()
    {
        ComputeShaderUtils.ResetTexture(SoilMap);
        ComputeShaderUtils.ResetTexture(SoilOutput);

        var kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.SetTexture(kernelId, "SoilMap", SoilMap);
        SoilShader.SetTexture(kernelId, "WaterMap", WaterMap);
        SoilShader.SetTexture(kernelId, "BedrockHeightMap", BedrockHeightMap);
        SoilShader.SetTexture(kernelId, "SoilHeightMap", SoilHeightMap);
        SoilShader.SetTexture(kernelId, "Result", SoilOutput);
    }

    void FixedUpdate()
    {
        int kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        Graphics.CopyTexture(SoilOutput, SoilMap);
        ComputeShaderUtils.InvalidateCache(SoilMap);
    }
}
