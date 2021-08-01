using UnityEngine;

public class EnvironmentDataStore : MonoBehaviour
{
    public static RenderTexture WaterSourceMap { get; set; }
    public static RenderTexture WaterMap { get; set; }
    public static RenderTexture LandMap { get; set; }
    public static RenderTexture ConntinentalHeightMap { get; set; }
    public static RenderTexture TectonicVelocityMap { get; set; }
    public static RenderTexture TectonicPlateIdMap { get; set; }

    void Awake()
    {
        LandMap = NewTexture(4);
        WaterMap = NewTexture(4);
        WaterSourceMap = NewTexture(4);
        ConntinentalHeightMap = NewTexture(1);
        TectonicVelocityMap = NewTexture(2);
        TectonicPlateIdMap = NewTexture(1);
    }

    private RenderTexture NewTexture(int channels)
    {
        var format = channels switch
        {
            1 => RenderTextureFormat.RFloat,
            2 => RenderTextureFormat.RGFloat,
            _ => RenderTextureFormat.ARGBFloat
        };
        return new RenderTexture(512, 512, 4, format, 0).ResetTexture().Initialize();
    }
}