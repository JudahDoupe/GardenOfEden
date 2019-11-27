using UnityEngine;

public class SoilService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture BedrockHeightMap;
    public RenderTexture SoilHeightMap;
    public RenderTexture SoilMap;
    public RenderTexture WaterMap;
    public RenderTexture RootGrowth;

    [Header("Compute Shaders")]
    public ComputeShader SoilShader;
    public ComputeShader SubtractShader;

    /* Pubically Accessable Methods */

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
        ComputeShaderUtils.ResetTexture(SoilMap);
        ComputeShaderUtils.ResetTexture(RootGrowth);

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
    }

    void FixedUpdate()
    {
        int kernelId = SoilShader.FindKernel("UpdateSoil");
        SoilShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }
}
