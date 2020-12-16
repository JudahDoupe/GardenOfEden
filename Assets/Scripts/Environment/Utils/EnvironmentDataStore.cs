using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class EnvironmentDataStore : MonoBehaviour
{
    public const int TextureSize = 512;

    public static RenderTexture WaterSourceMap;
    public static RenderTexture WaterMap;
    public static RenderTexture LandMap;
    public static RenderTexture SoilWaterMap;


    void Awake()
    {
        LandMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture();
        WaterMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture();
        SoilWaterMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture();
        WaterSourceMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture();
    }
}