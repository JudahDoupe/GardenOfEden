using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class EnvironmentDataStore : MonoBehaviour
{
    public const int TextureSize = 512;

    public static RenderTexture WaterSourceMap;
    public static RenderTexture WaterMap;
    public static RenderTexture LandMap;


    void Awake()
    {
        LandMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture().Initialize(-100, 0, 0, 0);
        WaterMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture().Initialize(0, 0, 100, 0);
        WaterSourceMap = new RenderTexture(512, 512, 4, GraphicsFormat.R32G32B32A32_SFloat, 0).ResetTexture().Initialize();
    }
}