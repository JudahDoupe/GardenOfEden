using UnityEngine;

public class ComputeShaderService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture RootMap;
    public RenderTexture Output;

    [Header("Compute Shaders")]
    public ComputeShader RootShader;
    public ComputeShader SubtractShader;

    /* Shader Methods */

    public Texture2D SpreadRoots(Texture2D currentRoots, Vector3 location, float radius, float depth)
    {
        if (currentRoots == null)
        {

            ComputeShaderUtils.ResetTexture(Output);
        }
        else
        {
            Graphics.Blit(currentRoots, Output);
        }
        var uv = ComputeShaderUtils.LocationToUv(location);
        int kernelId = RootShader.FindKernel("CSMain");
        RootShader.SetVector("RootData", new Vector4(uv.x, uv.y, radius, depth));
        RootShader.SetTexture(kernelId, "RootMap", RootMap);
        RootShader.SetTexture(kernelId, "Result", Output);
        RootShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
        return Output.ToTexture2D();
    }

    /* Inner Mechanations */

    void Start()
    {
        ComputeShaderUtils.ResetTexture(RootMap);
    }
}
