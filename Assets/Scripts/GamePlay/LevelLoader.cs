using System.Collections;
using System.IO;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Resource Maps")]
    public RenderTexture WaterMap;
    public RenderTexture SoilMap;

    [Header("")]
    public string MapName = "Hills";

    void Start()
    {
        StartCoroutine(LoadStarterLevel());
    }

    public void ExportLevel(string mapName)
    {
        Directory.CreateDirectory($"Assets/Resources/Map/{mapName}/");
        WaterMap.SaveToFile($"Assets/Resources/Map/{mapName}/water.tex");
        SoilMap.SaveToFile($"Assets/Resources/Map/{mapName}/soil.tex");
    }

    public void LoadLevel(string mapName)
    {
        WaterMap.LoadFromFile($"Assets/Resources/Map/{mapName}/water.tex");
        SoilMap.LoadFromFile($"Assets/Resources/Map/{mapName}/soil.tex");
    }

    private IEnumerator LoadStarterLevel()
    {
        yield return new WaitForSeconds(0.1f);
        LoadLevel(MapName);
    }
}
