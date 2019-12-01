using UnityEngine;

public class SoilService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture BedrockHeightMap;
    public RenderTexture SoilHeightMap;
    public RenderTexture SoilMap;
    public RenderTexture WaterMap;
    public RenderTexture WaterOutput;
    public RenderTexture RootInput;
    public RenderTexture RootGrowth;
    public RenderTexture SoilOutput;

    [Header("Compute Shaders")]
    public ComputeShader SoilShader;
    public ComputeShader SubtractShader;

    /* Pubically Accessable Methods */

    public float SampleSoilDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = SoilMap.ToTexture2D().GetPixelBilinear(uv.x, uv.y);
        return color.r;
    }

    public float SampleRootDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = SoilMap.ToTexture2D().GetPixelBilinear(uv.x, uv.y);
        return color.g;
    }

    public float SampleWaterDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = SoilMap.ToTexture2D().GetPixelBilinear(uv.x, uv.y);
        return color.b;
    }

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
        return WaterOutput.ToTexture2D();
    }

    public Texture2D SpreadRoots(Texture2D currentRoots, Vector3 location, float radius, float depth)
    {
        if (currentRoots != null)
        {
            Graphics.Blit(currentRoots, RootGrowth);
        }
        var uv = ComputeShaderUtils.LocationToUv(location);
        int kernelId = SoilShader.FindKernel("SpreadRoots");
        SoilShader.SetVector("RootData", new Vector4(uv.x, uv.y, radius, depth));
        SoilShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);

        return RootGrowth.ToTexture2D();
    }

    /* Inner Mechanations */

    void Start()
    {
        ComputeShaderUtils.ResetTexture(WaterOutput);
        ComputeShaderUtils.ResetTexture(RootInput);
        ComputeShaderUtils.ResetTexture(SoilMap);
        ComputeShaderUtils.ResetTexture(RootGrowth);
        ComputeShaderUtils.ResetTexture(SoilOutput);

        int kernelId = SoilShader.FindKernel("SpreadRoots");
        SoilShader.SetTexture(kernelId, "SoilMap", SoilMap);
        SoilShader.SetTexture(kernelId, "RootGrowth", RootGrowth);
        SoilShader.SetTexture(kernelId, "BedrockHeightMap", BedrockHeightMap);
        SoilShader.SetTexture(kernelId, "SoilHeightMap", SoilHeightMap);

        kernelId = SoilShader.FindKernel("UpdateSoil");
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
    }
}
