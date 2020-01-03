using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelExporter : MonoBehaviour
{
    public RenderTexture WaterMap;
    public RenderTexture SoilMap;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ExportLevel();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadLevel();
        }
    }

    private void ExportLevel()
    {
        var waterTex = WaterMap.ToTexture2D().EncodeToPNG();
        var soilTex = SoilMap.ToTexture2D().EncodeToPNG();

        var waterPath = EditorUtility.SaveFilePanelInProject("Export Level", "water.png", "png", "Save Level to folder/");
        var soilPath = EditorUtility.SaveFilePanelInProject("Export Level", "soil.png", "png", "Save Level to folder/");

        File.WriteAllBytes(waterPath, soilTex);
        File.WriteAllBytes(soilPath, soilTex);
    }

    private void LoadLevel()
    {
        var waterPath = EditorUtility.OpenFilePanel("Water Map", "Assets/Map", "png");
        var soilPath = EditorUtility.OpenFilePanel("Soil Map", "Assets/Map", "png");

        var waterTex = new Texture2D(ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize, TextureFormat.RGBAFloat, false);
        var soilTex = new Texture2D(ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize, TextureFormat.RGBAFloat, false);

        waterTex.LoadImage(File.ReadAllBytes(waterPath));
        soilTex.LoadImage(File.ReadAllBytes(soilPath));

        Graphics.Blit(waterTex, WaterMap);
        Graphics.Blit(soilTex, SoilMap);
    }
}
