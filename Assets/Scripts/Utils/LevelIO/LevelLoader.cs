using System.Collections;
using System.IO;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Resource Maps")]
    public RenderTexture WaterMap;
    public RenderTexture WaterSourceMap;
    public RenderTexture LandMap;
    public RenderTexture SoilWaterMap;

    [Header("Renderers")]
    public RenderTexture SoilHeightMap;
    public RenderTexture BedrockHeightMap;
    public RenderTexture WaterSourceHeightMap;
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
        WaterSourceMap.SaveToFile($"Assets/Resources/Map/{mapName}/waterSource.tex", TextureFormat.RFloat);
        LandMap.SaveToFile($"Assets/Resources/Map/{mapName}/land.tex");
        SoilWaterMap.SaveToFile($"Assets/Resources/Map/{mapName}/soilWater.tex");
    }

    public void LoadLevel(string mapName)
    {
        ResetMaps();
        WaterMap.LoadFromFile($"Assets/Resources/Map/{mapName}/water.tex");
        WaterSourceMap.LoadFromFile($"Assets/Resources/Map/{mapName}/waterSource.tex", TextureFormat.RFloat);
        LandMap.LoadFromFile($"Assets/Resources/Map/{mapName}/land.tex");
        SoilWaterMap.LoadFromFile($"Assets/Resources/Map/{mapName}/soilWater.tex");
    }

    public void RenderMaps()
    {
        SetRenderersEnabled(true);
        ResetMaps();
        var kernelId = RenderMapsShader.FindKernel("RenderMaps");
        RenderMapsShader.SetTexture(kernelId, "LandMap", LandMap);
        RenderMapsShader.SetTexture(kernelId, "SoilHeightMap", SoilHeightMap);
        RenderMapsShader.SetTexture(kernelId, "BedrockHeightMap", BedrockHeightMap);
        RenderMapsShader.SetTexture(kernelId, "WaterSourceMap", WaterSourceMap);
        RenderMapsShader.SetTexture(kernelId, "WaterSourceHeightMap", WaterSourceHeightMap);
        RenderMapsShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
    }
    public void SetRenderersEnabled(bool enable)
    {
        transform.Find("Renderers").gameObject.SetActive(enable);
        transform.Find("Meshes").Find(MapName).gameObject.SetActive(enable);
    }

    private void ResetMaps()
    {
        ComputeShaderUtils.ResetTexture(WaterMap);
        ComputeShaderUtils.ResetTexture(WaterSourceMap);
        ComputeShaderUtils.ResetTexture(LandMap);
        ComputeShaderUtils.ResetTexture(SoilWaterMap);
    }

    private IEnumerator LoadStarterLevel()
    {
        yield return new WaitForSeconds(0.1f);
        LoadLevel(MapName);
    }
}
