using System.Collections;
using System.IO;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Resource Maps")]
    public RenderTexture WaterMap;
    public RenderTexture LandMap;
    public RenderTexture SoilWaterMap;

    [Header("Renderers")]
    public RenderTexture SoilHeightMap;
    public RenderTexture BedrockHeightMap;
    public ComputeShader RenderMapsShader;

    [Header("")]
    public string MapName = "Hills";

    void Start()
    {
        LoadLevel(MapName);
        StartCoroutine(LoadStarterLevel());
    }

    public void SaveLevel(string mapName)
    {
        Directory.CreateDirectory($"Assets/Resources/Map/{mapName}/");
        WaterMap.SaveToFile($"Assets/Resources/Map/{mapName}/water.tex");
        LandMap.SaveToFile($"Assets/Resources/Map/{mapName}/land.tex");
        SoilWaterMap.SaveToFile($"Assets/Resources/Map/{mapName}/soilWater.tex");
    }

    public void LoadLevel(string mapName)
    {
        ComputeShaderUtils.ResetTexture(WaterMap);
        ComputeShaderUtils.ResetTexture(LandMap);
        ComputeShaderUtils.ResetTexture(SoilWaterMap);

        WaterMap.LoadFromFile($"Assets/Resources/Map/{mapName}/water.tex");
        LandMap.LoadFromFile($"Assets/Resources/Map/{mapName}/land.tex");
        SoilWaterMap.LoadFromFile($"Assets/Resources/Map/{mapName}/soilWater.tex");
    }

    public void RenderMaps()
    {
        var kernelId = RenderMapsShader.FindKernel("RenderMaps");
        RenderMapsShader.SetTexture(kernelId, "LandMap", LandMap);
        RenderMapsShader.SetTexture(kernelId, "SoilHeightMap", SoilHeightMap);
        RenderMapsShader.SetTexture(kernelId, "BedrockHeightMap", BedrockHeightMap);
        RenderMapsShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }

    private IEnumerator LoadStarterLevel()
    {
        yield return new WaitForSeconds(0.1f);
        LoadLevel(MapName);
    }
}
