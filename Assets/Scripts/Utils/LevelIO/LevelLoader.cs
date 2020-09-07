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
    public MeshFilter Land;

    void Start()
    {
        LoadLevel(MapName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadLevel(MapName);
        }
    }

    public void SaveLevel(string mapName)
    {
        Directory.CreateDirectory($"Map/{mapName}/");
        WaterMap.SaveToFile($"Map/{mapName}/water.tex");
        WaterSourceMap.SaveToFile($"Map/{mapName}/waterSource.tex", TextureFormat.RFloat);
        LandMap.SaveToFile($"Map/{mapName}/land.tex");
        SoilWaterMap.SaveToFile($"Map/{mapName}/soilWater.tex");
    }

    public void LoadLevel(string mapName)
    {
        ResetMaps();
        WaterMap.LoadFromFile($"Map/{mapName}/water.tex");
        WaterSourceMap.LoadFromFile($"Map/{mapName}/waterSource.tex", TextureFormat.RFloat);
        LandMap.LoadFromFile($"Map/{mapName}/land.tex");
        SoilWaterMap.LoadFromFile($"Map/{mapName}/soilWater.tex");
        Land.mesh.bounds = new Bounds(Land.mesh.bounds.center, new Vector3(Land.mesh.bounds.size.x, 500, Land.mesh.bounds.size.z));
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
        WaterMap.ResetTexture();
        WaterSourceMap.ResetTexture();
        LandMap.ResetTexture();
        SoilWaterMap.ResetTexture();
    }
}
