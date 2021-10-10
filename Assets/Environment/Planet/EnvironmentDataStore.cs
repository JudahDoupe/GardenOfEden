using UnityEngine;

public class EnvironmentDataStore : MonoBehaviour
{
    public static RenderTexture WaterSourceMap { get; set; }
    public static RenderTexture WaterMap { get; set; }
    public static RenderTexture LandHeightMap { get; set; }
    public static RenderTexture PlateThicknessMaps { get; set; }
    public static RenderTexture TmpPlateThicknessMaps { get; set; }
    public static RenderTexture ContinentalIdMap { get; set; }
    void Awake()
    {
        WaterMap = NewTexture(4, 6);
        WaterSourceMap = NewTexture(4, 6);
        LandHeightMap = NewTexture(1, 6);
        PlateThicknessMaps = NewTexture(1, 1);
        TmpPlateThicknessMaps = NewTexture(1, 1);
        ContinentalIdMap = NewTexture(1, 6);
    }

    private RenderTexture NewTexture(int channels, int layers)
    {
        var format = channels switch
        {
            1 => RenderTextureFormat.RFloat,
            2 => RenderTextureFormat.RGFloat,
            _ => RenderTextureFormat.ARGBFloat
        };
        return new RenderTexture(512, 512, 0, format, 0).ResetTexture(layers).Initialize();
    }
}